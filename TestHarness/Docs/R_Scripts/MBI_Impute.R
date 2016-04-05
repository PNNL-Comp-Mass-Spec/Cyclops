
# Model-based imputaion
# Ref:  "A statistical framework for protein quantitation in bottom-up MS-based
#        proteomics. Karpievitch Y, Stanley J, Taverner T, Huang J, Adkins JN,
#        Ansong C, Heffron F, Metz TO, Qian WJ, Yoon H, Smith RD, Dabney AR.
#        Bioinformatics 2009
#
# Written by Yuliya Karpievitch, Tom Taverner, and Shelley Herbrich
# for Pacific Northwest National Lab (PNNL) and community

#   source('MBfilter.r')
#   source('MBImpute.r')
#   fnameLum = 'D:/yuliya/pnnl_cod/my_functions/data/proteins2_lumican_anthithr.txt'
#   datasetLum <- read.table(fnameLum, header=TRUE, sep="\t")
#   trLum <- c(1,1,1,1,1,1,1,1,1,1, 2,2,2,2,2,2,2,2,2,2)
#   resMBfilter <- MBfilter(datasetLum[,-(1:2)], trLum, datasetLum[,1:2], pr_group=2, my.pi=.05)
#
#   resMBimpute <- MBimpute(resMBfilter$y_filtered,trLum,resMBfilter$ft_prot.info,pr_group=2)


MBimpute = function(mm, treatment,prot.info,pr_group=2,my.pi=0.05, compute_pi=TRUE){
# Imputes missing values 
# Expects the data to be filtered with model-based filtering prior to imputation
# Handles no sibling peptides with missing groups (think not this version)
# 
# Input:
#   mm:    m x n matrix of intensities, numpeptides x numsamples
#   treatment:  vector indicating the treatment group of each sample ie [1 1 1 1 2 2 2 2...]
#   pr_group - column index for protein ID in prot.info. Can restrict to be #2...
#   my.pi: PI value, estimate of the proportion of peptides missign completely at random,
#          as compared to censored at lower abundance levels
#          default values of 0.05 is usually reasoanble, unless some filtering was done before
#   compute_pi=TRUE - estimate PI, or use the default

# Output: list of:
#   y_imputed: k x n matrix of 'complete' data (original and imputed values for missing data),
#             k <= m, as some peptides may have been filtered out due to low information content
#   imp_prot.info: imputed protein info, 2 col: pepIDs and prIDs
#                  Should be the same as passed in as MBfilter should have removed all 'low Q'
#                  peptides/proteins.
#   ? p-values computed after model-based filtering and imputation


# yuliya: not in stand-alone version
# prot.info <- get.ProtInfo(m)  # pepID, prID - not as in our text file data frame? here a mat
# prot.info <- cbind(prot.info[,1], prot.info[,protein_group])
# treatment <- get.factors(m)[,treatment]

# yuliya: this is only used at end to assign metadata, standalone have to keep track otherwise
# row metadata will be pepIDs and prIDs
# obj_metadata <- list(attr(m, "Row_Metadata"), attr(m, "Column_Metadata"))
# obj_dimnames <- dimnames(m)

  # calculate PI or used one passed in as parameter:
  if (!compute_pi){
  	my.pi <- eigen_pi(m, toplot=T)
  } # else should be taken from the user interface, can we verify it?

  # treatment factors
  n.treatment <- length(treatment)
  n.u.treatment <- length(unique(treatment))

  # Match to protein
  all.proteins <- unique(prot.info[,2])
  # yuliya: do NOT want to order, this is from Matlab that sorts Unique values, here we can perserve original order
  # all.proteins <- all.proteins[order(all.proteins)]

  y_imputed <- NULL
  imp_prot.info <- NULL
  cat("Imputing...\n")

  for (k in 1:length(all.proteins)){
    prot <- all.proteins[k]
    pmid.matches <- prot.info[prot.info[,pr_group]==prot,1]
    curr_prot.info <- prot.info[prot.info[,pr_group]==prot,] # NEEDED??
    ## idx.prot <- which(rownames(data) %in% pmid.matches)
    # idx.prot <- which(rownames(m) %in% pmid.matches)
    idx.prot <- which(prot.info[,1] %in% pmid.matches)  # do not obligate
                              # to be rownames, as those have to be Unique
    # y_raw <- rbind(data[idx.prot,])
    y_raw <- mm[idx.prot,,drop=F]
    cat(paste("Protein: ", prot, ": ", dim(y_raw)[1], " peptides (", k, "/", length(all.proteins), ")", sep="" ),"\n")
    y_info <- prot.info[idx.prot,,drop=F]
    # rownames(y_raw) <- rownames(data)[idx.prot]
    # rownames(y_raw) <- rownames(m)[idx.prot]

    if (nrow(y_raw) == 0) next  # yuliya: this should not happen here (NO observations)
    
    #peptides and proteins of poor quality are removed prior to analysis 
    # "poor quality" is defined as having little "information" abt grp differences compared to other peptides
    # estimate data parameters

    n.peptide <- nrow(y_raw)
    y <- as.vector(t(y_raw))
    n <- length(y)
    peptide <-rep(rep(1:n.peptide, each=n.treatment))

    # filter out min.missing
    n.present <- array(NA, c(n.peptide, n.u.treatment))
    colnames(n.present) <- unique(treatment)
    # yuliya: not sure what the difference is, this works in filtering.
    # for(i in 1:n.peptide) for(j in 1:n.u.treatment)
    #    n.present[i,j] <- sum(!is.na(y [peptide==i & treatment==unique(treatment)[j]]))
    for(jj in 1:n.u.treatment)
       n.present[,jj] <- rowSums(!is.na(y_raw[,treatment==unique(treatment)[jj],drop=F]))

    # remove peptides with completely missing group(s)
    present.min <- apply(n.present, 1, min)
    ii <- present.min > 0
    y_raw <- y_raw[ii,,drop=F] # reassign Y_raw to a submatrix of 1+ observations in each group

    # yuliya: try to keep track of pepIDs and prIDs here...
    curr_prot.info <- curr_prot.info[ii,] # possibly a subset
    
    if (nrow(y_raw) == 0) next

    # re-evaluate data parameters after filtering
    n.peptide <- nrow(y_raw)
    y <- as.vector(t(y_raw))
    n <- length(y)
    c.guess <- min(y, na.rm=T)
    peptide <-rep(rep(1:n.peptide, each=n.treatment))

    # yuliya: this is commented out in filtering... leaving it alone for now...
    # re-calculate number of observed values per treatment group for each peptide
    # yuliya: may be able to use previous computation here for efficiency... but as long
    # as this works do not care to change at this time
    n.present <- array(NA, c(n.peptide, n.u.treatment))
    colnames(n.present) <- unique(treatment)
    for(i in 1:n.peptide){
      for(j in 1:n.u.treatment){
        n.present[i,j] <- sum(!is.na(y [peptide==i & treatment==unique(treatment)[j]]))
      }
    }

    # calculate pooled variance for each protein
    grp <- array(NA, c(1, n.u.treatment))
    for (j in 1:n.u.treatment){
      grp[j] <- sum(n.present[, j])
    }
    mpos <-which.max(grp)
    pep_var <- 0 # yuliys was not declared in impute only
    go <- protein_var(y_raw, treatment) # yuliya: function, see below
    overall_var <- go$overall_var

    num <-0
    den <- 0

    # calculate pooled variance for each peptide;
    # if only 1 onservation in a dx group assign the overall variance
    for(i in 1:n.peptide){
      y.i <- na.omit(y[peptide==i & treatment==unique(treatment)[mpos]])
      p2 <- var(y.i)
      if (is.na(p2)) p2 <- 0
      present <- length(y.i)
      num <- num+(p2*(present-1))
      den <- den + (present-1)
    }
    pep_var <- num/den
    if (pep_var==0) pep_var <- overall_var

    peptides.missing <- rowSums(is.na(y_raw))

    f.treatment <- factor(rep(treatment, n.peptide))
    f.peptide <- factor(peptide)

    # estimate rough model parameters
    # create model matrix for each protein and
    # remove any peptides with missing values
    ii <- (1:n)[is.na(y)]
    if (n.peptide != 1){
      X  <- model.matrix(~f.peptide + f.treatment, contrasts = list(f.treatment="contr.sum", f.peptide="contr.sum") )
    } else {
      X <- model.matrix(~f.treatment, contrasts=list(f.treatment="contr.sum"))
    }
    if(length(ii) > 0){
      y.c <- y[-ii]
      X.c <- X[-ii,]
    } else {
      y.c <- y
      X.c <- X
    }

    # calculate initial beta values and residuals
    beta <- drop(solve(t(X.c) %*% X.c) %*% t(X.c) %*% y.c)

    # compute initial delta's
    peptides.missing[peptides.missing==0] <- 0.9
    delta.y <- as.numeric(1/sqrt(pep_var*peptides.missing))
    dd <- delta.y[as.numeric(peptide)]


    # calculate cutoff values for each peptide
    c_hat = rep(NA, n.peptide)
    for(j in 1:n.peptide) {
      c_hat[j] = min(y_raw[j, ], na.rm = T)
    }
    c_h <- c_hat[as.numeric(peptide)]

    if(n.peptide==1){
      y.predict <- model.matrix(~f.treatment, contrasts=list(f.treatment="contr.sum"))%*% beta
    } else {
      y.predict <- model.matrix(~f.peptide + f.treatment,contrasts = list(f.treatment="contr.sum", f.peptide="contr.sum"))%*% beta
    }

    zeta <- dd*(c_h - y.predict)
    prob.cen <- pnorm(zeta, 0, 1)/(my.pi + (1-my.pi)*pnorm(zeta, 0, 1))

    choose.cen <- runif(n) < prob.cen
    set.cen <- is.na(y) &choose.cen
    set.mar <- is.na(y) &!choose.cen
    kappa <- my.pi + (1 - my.pi)*dnorm(zeta,0, 1)

    # yuliya, this is nto needed in imputation....
    # # compute information
    # I_beta <- t(X) %*% diag(as.vector(dd^2*(1 - kappa*(1 + my.Psi.dash(zeta, my.pi))))) %*% X
    # I_GRP <- I_beta[rev(rev(1:n.peptide+1)[1:(n.u.treatment - 1)]), rev(rev(1:n.peptide+1)[1:(n.u.treatment - 1)]), drop = F]
    # if (!is.null(dim(I_GRP))) I_GRP = det(I_GRP)

    # Imputation: Replace missing values with random numbers drawn from the estimated likelihood model
    sigma <- 1/dd
    y.impute <- t(y_raw)
    if(sum(set.cen) > 0)
      y.impute[set.cen] <- rnorm.trunc(sum(set.cen), y.predict[set.cen], sigma[set.cen], hi=rep(c.guess, n)[set.cen])

    if(sum(set.mar) > 0)
      y.impute[set.mar] <- rnorm(n, y.predict, sigma)[set.mar]
      
    y.impute.return <- t(y.impute)
    prot.info.tmp <- curr_prot.info

    #y.impute.ret <- cbind(y_info, y.impute.return)


#browser()

    #row.names(y.impute.ret) <- row.names(y_raw)
    # tryCatch(y_imputed <- rbind(y_imputed, y.impute.return), error = function(e){
    tryCatch(y_imputed <- rbind(y_imputed, y.impute.return), imp_prot.info <- rbind(imp_prot.info,prot.info.tmp), error = function(e){
      print(y.impute.return)
      print(dim(y_imputed))
      print(dim(y.impute.return))      
      stop("Error in rbind() for y.impute.net")      
    })
    
  } #end for each protein
  
  #y_imputed <- y_imputed[,-c(1,2), drop=FALSE]   # remove pr and pep ids from the datadrame, make a matrix
  #y_imputed_dimnames <- dimnames(y_imputed)

  #y_imputed <- array(as.numeric(y_imputed), dim(y_imputed))
  # y_imputed <- data.matrix(y_imputed) # as.matrix destroys the row/col info, so do that 1st

  #colnames(y_imputed) <-  obj_dimnames[[2]]
  #row.names(y_imputed) <-  y_imputed_dimnames[[1]]

  # yuliya: stand-alone - do not have attr, need to add info back, cols only?
  # attr(y_imputed, "Row_Metadata") <- obj_metadata[[1]]
  # attr(y_imputed, "Column_Metadata") <- obj_metadata[[2]]
  colnames(y_imputed) <- colnames(mm)

  cat("Done imputing.\n")
  return(list(y_imputed=y_imputed,imp_prot.info=imp_prot.info,pi=as.matrix(my.pi)))
}
############# end imputation ###################
############## function pi #############

if(0){
eigen_pi <- function(m, toplot=T){ 
# Compute PI - proportion of observations missing at random
# INPUT: m - matrix of abundances, numsmaples x numpeptides
#       toplot - T/F plot mean vs protportion missing, curve and PI 
# OUTPUT: pi - 
# 
# Shelley Herbrich, June 2010, created for EigenMS and TamuQ 
#
# (1) compute 1) ave of the present values from each petide
#             2) number of missing and present values for each peptide
  
  # m = m[,-(1:2)] # yuliya: no 2 columns of ids, already on log scale
  # m = log(m)
  #remove completely missing rows
  m = m[rowSums(m, na.rm=T)!=0,]

  pepmean <- apply(m, 1, mean, na.rm=T)
  propmiss <- rowSums(is.na(m))/ncol(m)

  smooth_span <- (0.4)
  fit <- lowess(pepmean, propmiss, f=smooth_span)
  PI <- fit$y[fit$x==max(pepmean)]

  count <- 1
  while (PI<=0){
    smooth_span <- smooth_span-.1
    fit <- lowess(pepmean, propmiss, f=smooth_span)
    PI <- fit$y[fit$x==max(pepmean)]
    count <- count + 1
    if (count > 500) break
  }

  if (toplot){
  st <- paste("PI: ", PI) 
  plot(pepmean, propmiss, xlab="x", ylab="y", cex=0.5) #plot data point
  lines(fit)
  title("Lowess Regression", sub = st,
      cex.main = 2,   font.main= 3, col.main= "purple",
      cex.sub = 1, font.sub = 3, col.sub = "red")
  }
  return (pi=PI)
}

}
######################################################
protein_var <- function(Y_raw, treatment){
# estimates coefficients for all peptides from a single protein
# a portion of what get_coeffs does , without information computations
# that is nto needed in imputation
#
# Input:
#   Y_raw: m peptides by n samples arrays matrix of expression data
#          from a given protein
#   treatment: treatment groups
#
# Output:
#   overall_var:  need to check...

  n.peptide <- nrow(Y_raw)
  y <- as.vector(t(Y_raw))
  n <- length(y)
  n.treatment <- length(treatment)
  n.u.treatment <- length(unique(treatment))
  peptide <-rep(rep(1:n.peptide, each=n.treatment))

  n.present <- array(NA, c(n.peptide, n.u.treatment))
  colnames(n.present) <- unique(treatment)
  for(i in 1:n.peptide) for(j in 1:n.u.treatment) n.present[i,j] <- sum(!is.na(y [peptide==i & treatment==unique(treatment)[j]]))

  peptides.missing <- rowSums(is.na(Y_raw))

  f.treatment <- factor(rep(treatment, n.peptide))
  f.peptide <- factor(peptide)

  # estimate rough model parameters
  # create model matrix for each protein and
  # remove any peptides with missing values
  ii <- (1:n)[is.na(y)]
  if (n.peptide != 1){
    X  <- model.matrix(~f.peptide + f.treatment, contrasts = list(f.treatment="contr.sum", f.peptide="contr.sum") )
  } else {
    X <- model.matrix(~f.treatment, contrasts=list(f.treatment="contr.sum"))
  }
  if(length(ii) > 0){
    y.c <- y[-ii]
    X.c <- X[-ii,]
  } else {
    y.c <- y
    X.c <- X
  }

  # calculate initial beta values and residuals
  beta <- drop(solve(t(X.c) %*% X.c) %*% t(X.c) %*% y.c)
  Y_hat <- X.c %*% beta
  Y_temp <- Y_raw
  # Y_temp <- as.numeric(t(Y_temp))
  Y_temp <- as.vector(t(Y_temp)) # yuliya, same as in filter now
  Y_temp[!is.na(Y_temp)] <- Y_hat
  Y_temp <- matrix(Y_temp, nrow = n.peptide, byrow = T)
  Y_hat <- Y_temp
  ee <- Y_raw - Y_hat

  effects <- X.c %*% beta
  resid <- y.c - effects
  overall_var <- var(resid)
  return(list(overall_var=det(overall_var)))
}
################################################
my.Psi = function(x, my.pi){
# calculates Psi
exp(log(1-my.pi)  + dnorm(x, 0, 1, log=T) - log(my.pi + (1 - my.pi) * pnorm(x, 0, 1) ))
}
# end my.Psi

my.Psi.dash = function(x, my.pi){
# calculates the derivative of Psi
-my.Psi(x, my.pi) * (x + my.Psi(x, my.pi))
}
# end my.Psi.dash

phi = function(x){dnorm(x)}

rnorm.trunc <- function (n, mu, sigma, lo=-Inf, hi=Inf){
# Calculates truncated noraml
  p.lo <- pnorm (lo, mu, sigma)
  p.hi <- pnorm (hi, mu, sigma)
  u <- runif (n, p.lo, p.hi)
  return (qnorm (u, mu, sigma))
}
# End rnorm.trunc



################################################
# dialog for DanteR
################################################
MBimpute.dialog = list( title='Model-based Imputation',
  m.dataframeItem=NULL, label='Data to impute',
  signal = list("default", "get.dataset.factors", "treatment", user.data=list(include.primary=FALSE)),
  signal = list("default", "get.dataset.row.metadata.fields", "protein_group", user.data=list(include.primary=FALSE)),    
  treatment.choiceItem=NULL, label='Factors',
  protein_group.choiceItem = NULL, label = "Field Containing Proteins",   
  compute_pi.trueFalseItem=FALSE, label='Manually Estimate PI',
   tooltip = "If this is checked, the user can set the estimated random missingness probability",
  signal = c("default", "toggle.sensitive", "my.pi"),
    my.pi.numericItem="0.05", label='PI', indent=10
)




# get_pi = function (choice){
		# if (choice=="Yes"){
		# my.pi = my.pi
	# }else {
		# my.pi = run.dialog(enter_pi)$retval
	# }
  # return(my.pi)
# }

# enter_pi = function(new_val){
	# my.pi = new_val
	# return(my.pi)
# }
# enter_pi.dialog=list(title='New PI Value', new_val.numericItem=NULL, label='Enter new PI value:')



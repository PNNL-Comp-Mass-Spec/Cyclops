
# Model-based filtering (likelihood model)
# Ref:  "A statistical framework for protein quantitation in bottom-up MS-based
#        proteomics. Karpievitch Y, Stanley J, Taverner T, Huang J, Adkins JN,
#        Ansong C, Heffron F, Metz TO, Qian WJ, Yoon H, Smith RD, Dabney AR.
#        Bioinformatics 2009
#
# Written by Yuliya Karpievitch, Tom Taverner and Shelley Herbrich
# for Pacific Northwest National Lab (PNNL) and community

#   source('MBfilter.r')
#   fnameLum = 'D:/yuliya/pnnl_cod/my_functions/data/proteins2_lumican_anthithr.txt'
#   datasetLum <- read.table(fnameLum, header=TRUE, sep="\t")
#   trLum <- c(1,1,1,1,1,1,1,1,1,1, 2,2,2,2,2,2,2,2,2,2)
#   resMBfilter <- MBfilter(datasetLum[,-(1:2)], trLum, datasetLum[,1:2], pr_group=2, my.pi=.05)


# Arguments: data frame m, treatment being the group
# compute_pi is true if the user enters pi, topNpep is top N searched
MBfilter <- function(mm, treatment,prot.info,pr_group=2,my.pi=0.05,compute_pi=TRUE,topNPep=1E6,compute_pi_only = FALSE){
# filter low quality prptides
#
# Input:
#   mm: An m x n matrix of intensities, numpeptides x numsamples
#   treatment:  vector indicating the treatment group of each sample ie [1 1 1 1 2 2 2 2...]
#   prot.info = data.2+ column natrix with: col1 - pepID, col2 - prID. prID can be any other column but 1.
#   pr_group - column index for protein ID in prot.info. Can restrict to be #2...
#   my.pi: PI value, estimate of the proportion of peptides missign completely at random,
#          as compared to censored at lower abundance levels
#          default values of 0.05 is usually reasoanble, unless some filtering was done before
#   compute_pi=TRUE - estimate PI, or use the default,
#   topNPep=1E6 - max number of peptides to use if one wants to save time, dafault are all,
#   compute_pi_only - estimate PI only, use can change the value of PI if automatic estimation fails
#
# Output: list of:
#   y_filtered: k x n matrix of filtered peptide intensities, k <= m, as some
#               peptides may have been filtered out due to low information content
#   ft_prot.info: filtered protein info, 2 col: pepIDs and prIDs
#   pi - estimate of PI used


  # prot.info <- get.ProtInfo(m)  # pepID, prID - not as in our text file data frame? here a mat
  # prot.info <- cbind(prot.info[,1], prot.info[,protein_group])
  # treatment <- get.factors(m)[,treatment] # yuliya: treatment is already this
  n.treatment <- length(treatment)
  n.u.treatment <- length(unique(treatment))
  
  # yuliya: this is only used at end to assign metadata, standalone have to keep track otherwise
  # row metadata will be pepIDs and prIDs
  # obj_metadata <- list(attr(m, "Row_Metadata"), attr(m, "Column_Metadata"))
  
  all.proteins <- unique(prot.info[,pr_group])  # 2]) - this does nto tak parameter into account
  # yuliya: do NOT want to order, this is from Matlab that sorts Unique values, here we can perserve original order
  #  all.proteins <- all.proteins[order(all.proteins)]

  # rownames(m) <- prot.info[,1]
  y_filtered <- NULL
  ft_prot.info <- NULL
  
  if (!compute_pi){ my.pi <- eigen_pi(mm, toplot=TRUE) } # estimate PI if needed
  cat(paste("Estimated PI:", signif(my.pi, 3), "\n"))
  if(compute_pi_only) return(list(pi=as.matrix(my.pi)))
  
  cat("Filtering...\n")
  
  
  
  for (k in 1:length(all.proteins)){
    # Not sure why this does strange things... 082410 Tom's comment? what strange things?
  tryCatch({
    prot <- all.proteins[k]
    pmid.matches <- prot.info[prot.info[,pr_group]==prot,1]
    curr_prot.info <- prot.info[prot.info[,pr_group]==prot,] # all cols

    #idx.prot <- which(rownames(m) %in% pmid.matches) # yulya, comment line out
    idx.prot <- which(prot.info[,1] %in% pmid.matches)  # do not obligate
                              # to be rownames, as those have to be Unique
    #    y_raw <- rbind(data[idx.prot,])
    y_raw <- mm[idx.prot,,drop=F]   # this is 1 protein with all peptides

    cat(paste("Protein: ", prot, ": ", dim(y_raw)[1], " peptides (", k, "/", length(all.proteins), ")", sep="" ))
    y_info <- prot.info[idx.prot,,drop=F]

    # yuliya: this requires pepIDs to be unique, so may be too constrictive?
    # rownames(y_raw) <- rownames(mm)[idx.prot]
    
    if (nrow(y_raw) == 0) {  # yuliya: what is this??? can there be NO observations?
      cat("\n")    
      next
    }


    # Peptides and proteins of "poor quality" are removed prior to analysis
    # "poor quality" is defined as having little "information" abt grp differences compared to other peptides
    # Filter y_raw, estimate data parameters
    n.peptide <- nrow(y_raw)
    
    y <- as.vector(t(y_raw))
    n <- length(y)
    peptide <-rep(rep(1:n.peptide, each=n.treatment))    

    # calculate number of observed values per treatment group for each peptide
    n.present <- array(NA, c(n.peptide, n.u.treatment))

    colnames(n.present) <- unique(treatment)    

    for(jj in 1:n.u.treatment)    
       n.present[,jj] <- rowSums(!is.na(y_raw[,treatment==unique(treatment)[jj],drop=F]))
       

    # remove peptides with completely missing group(s)
    present.min <- apply(n.present, 1, min)
    ii <- present.min > 0
    n.present <- n.present[ii,,drop=F]
    y_raw <- rbind(y_raw[ii,,drop=F]) # reassign Y_raw to a submatrix of 1+ observations in each group
    
    # yuliya: try to keep track of pepIDs and prIDs here...
    curr_prot.info <- curr_prot.info[ii,] # possibly a subset

    if (nrow(y_raw) == 0) { # yuliya: this one I understand, may have removed ALL peptides...
      cat("\n")    
      next
    }

    # re-evaluate data parameters after filtering out peptides with 1+ group missing
    n.peptide <- nrow(y_raw)
    y <- as.vector(t(y_raw))
    n <- length(y)
    c.guess <- min(y, na.rm=T)
    peptide <-rep(rep(1:n.peptide, each=n.treatment))
    
    # TAKEN care of ABOVE
    # re-calculate number of observed values per treatment group for each peptide
    # yuliya: may be able to use previous computation here for efficiency... but as long
    # as this works do not care to change at this time
#    n.present <- array(NA, c(n.peptide, n.u.treatment))
#    colnames(n.present) <- unique(treatment)
#    for(i in 1:n.peptide) {
#      for(j in 1:n.u.treatment) {
#        n.present[i,j] <- sum(!is.na(y [peptide==i & treatment==unique(treatment)[j]]))
#      }
#    }
#
    # calculate pooled variance for each protein
    grp <- array(NA, c(1, length(unique(treatment)))) # n.u.treatment
    for (j in 1:n.u.treatment){
      grp[j] <- sum(n.present[, j])
    }
    mpos <-which.max(grp)
    pep_var <- 0
    go <- get_coefs(y_raw, my.pi, pep_var, treatment) # yuliya: function, see below
    overall_var <- go$overall_var

    num <- 0
    den <- 1 # yuliya: not the best idea to set den - 0 as default, 1 is better

    # calculate pooled variance for each peptide;
    # if only 1 onservation in a dx group assign the overall variance
    for(i in 1:n.peptide) {
      y.i <- na.omit(y[peptide==i & treatment==unique(treatment)[mpos]])
      p2 <- var(y.i)
      if (is.na(p2)) p2 <- 0
      present <- length(y.i)
      num <- num+(p2*(present-1))
      den <- den + (present-1)
    }
    pep_var <- num/den
    
    # Not sure under which conditions pep_var will vanish? # Tom 082310
    if (!length(pep_var) || is.na(pep_var) || pep_var < 1E-3) {
      pep_var <- overall_var # testing for "== 0" is a bad idea
    }

    # estimate rough parameters based on present data
    gc <- get_coefs(y_raw, my.pi, pep_var, treatment) # this differs from go because pep_var is different;
    # use the info generated by get.coefs above to the overall_var, kind of a shortcut...

    C <- gc$I_GRP   # information content with ALL peptides
    C_cutoff <-C*0.9 # 90% of information explained by the peptides

    # greedy search algorithm to find peptides that capture 90% of information in a protein
    # this eliminates peptides that do not add much "information" to the protein,
    # these may be peptides with many missing values or false IDs that do not
    # have the same pattern as the rest of the peptides
    best.ind <- NULL
    grp.info.best <-NULL

    best <- 0

    while(best < C_cutoff && length(setdiff(1:nrow(y_raw), best.ind)) > 0 && length(best.ind) <= topNPep){
      
      grp.info <- rep(NA, nrow(y_raw))
      for(i in setdiff(1:nrow(y_raw), best.ind)){
        y.raw1 <- y_raw[c(i, best.ind),,drop=F]
        gc2 <- get_coefs(y.raw1, my.pi, pep_var, treatment)
        grp.info[i] <- gc2$I_GRP
      }

      best.ind <- c(best.ind, which(grp.info == max(grp.info, na.rm=T)))      
      cat(".") 
      flush.console()

      grp.info.best <- c(grp.info.best, max(grp.info, na.rm=T))
      best <- max(grp.info.best, na.rm=T)
      if(length(best) && is.na(best)) { # changed 092210
        cat("\nMBFilter: max(grp.info, na.rm=T) returned NA\n")
        break;
      } 

    }
    # y_filtered contains the filtered data set
    y_filter <- y_raw[best.ind,,drop=F]
    browser()
    prot.info.tmp <- curr_prot.info[best.ind,]
    y_filtered <- rbind(y_filtered, y_filter)

    # yuliya: stand-salone: keep track of prot.info
    ft_prot.info <- rbind(ft_prot.info,prot.info.tmp)
    
    
    cat("\n")
    flush.console()    
    }, error = function(e) {
      print(e)
    })

  } # end for each protein
  
  
  # yuliya: stand-alone - do not have attr, need to add info back, cols only?
  # attr(y_filtered, "Row_Metadata") <- obj_metadata[[1]]
  # attr(y_filtered, "Column_Metadata") <- obj_metadata[[2]]
  colnames(y_filtered) <- colnames(mm)
  # yuliya: row names are in ft_prot,infoin same order
  
  cat("Done filtering.\n")  
  return(list(y_filtered=y_filtered,ft_prot.info=ft_prot.info,pi=as.matrix(my.pi)))
}
##############################################################




##############################################################
get_coefs <- function(Y_raw, my.pi, pep_var, treatment){
# estimates coefficients and information for all peptides from a single protein
# 
# Input:
#   Y_raw: m peptides by n samples arrays matrix of expression data
#          from a given protein
#   my.pi: PI value, prob peptide is missing completely at random
#   pep_var: the pooled variance for each peptide (was protein???)
#   treatment: treatment groups
#  
# Output:
#   coef: NOT returend in FILTERING!  The size of the effect
#         THIS BETTER BE WRONG!!! -> (contrasted to the first condition)
#         Tom's favourite, but here we do not want to contrast agians ANY condition as a control!
#   I_GRP: the determinant of the information criteria (used for greedy search algorithm)
#   overall_var: yuliya: not sure why we return thsi here... need to check

# print("get_coefs")
  n.peptide <- nrow(Y_raw)
  y <- as.vector(t(Y_raw))
  n <- length(y)
  n.treatment <- length(treatment)
  n.u.treatment <- length(unique(treatment))
  peptide <-rep(rep(1:n.peptide, each=n.treatment))
  c.guess <- min(y, na.rm=T)
  
  ## catch NULL vectors
  ## depricated, we filter out these vectors; NEED to remove
  #  if (n.peptide < 1){
  #    return(list(n = 0, p.val = NULL, coef = NULL))
  #  }

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
  Y_temp <- as.vector(t(Y_temp))# tom changed from as.numeric
  Y_temp[!is.na(Y_temp)] <- Y_hat[!is.na(Y_temp)]
  Y_temp <- matrix(Y_temp, nrow = n.peptide, byrow = T)
  Y_hat <- Y_temp
  ee <- Y_raw - Y_hat
  effects <- X.c %*% beta
  resid <- y.c - effects
  overall_var <- var(resid)
  if (pep_var < 1E-3){
    pep_var <- overall_var
    #return(list(overall_var=det(overall_var)))# tom=
  }


  # compute initial delta's
  peptides.missing[peptides.missing==0] <- 0.9
  delta.y <- as.numeric(1/sqrt(pep_var*peptides.missing))
  dd <- delta.y[as.numeric(peptide)]

  # calculate cutoff values for each peptide
  c_hat = rep(NA, n.peptide)
  for(j in 1:n.peptide) {
    c_hat[j] = min(Y_raw[j, ], na.rm = T)
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

  #I_beta <- t(X) %*% diag(as.vector(dd^2*(1 - kappa*(1 + my.Psi.dash(zeta, my.pi)))))   %*% X
  
  # compute information
  Xt <- t(X)
  DD1 <- as.vector(dd^2*(1 - kappa*(1 + my.Psi.dash(zeta, my.pi))))
  for(jj in 1:dim(Xt)[2])
    Xt[,jj] <- Xt[,jj]*DD1[jj] 
  I_beta <- Xt %*% X  
  
  # I_GRP <- I_beta[rev(rev(1:n.peptide+1)[1:(n.u.treatment - 1)]), rev(rev(1:n.peptide+1)[1:(n.u.treatment - 1)]), drop = F]
  ridx <- -(1:n.peptide)
  I_GRP <- I_beta[ridx, ridx, drop = F]

  if (!is.null(dim(I_GRP))) I_GRP = det(I_GRP)
  a <- list(I_GRP=I_GRP, overall_var=overall_var)
    return(a)
}
######################## end get_coefs ###############################

# Note to Tom: this has NOTHING to do with "Eigen" anything...
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
  #st <- paste("PI: ", signif(PI, 3))
  plot(pepmean, jitter(propmiss), cex=0.5, pch=ifelse(length(pepmean) < 500, 1, "."), xlab = "Mean Value", ylab = "Pr(Data Missingness)") #plot data point
  abline(h=PI, col="red", lwd=2, lty=2)
  text(min(pepmean), PI+0.05, eval(parse(text=paste("expression(pi ==", signif(PI, 3), ")"))), cex=1.5, col="red", adj=0)
  lines(fit, lwd=2)
  title("Lowess Regression for Data Missingness", #sub = st,
      cex.main = 1.5,   font.main= 3, col.main= "blue",
      cex.sub = 1, font.sub = 3, col.sub = "red")
  }
  return (pi=PI)
}
###################  end eigen_pi  ########################







#######################################################################
# small helper functions
my.Psi = function(x, my.pi)
{
  # calculate Psi
  exp(log(1-my.pi)  + dnorm(x, 0, 1, log=T) - log(my.pi + (1 - my.pi) * pnorm(x, 0, 1) ))
} # end my.Psi


my.Psi.dash = function(x, my.pi)
{
  # calculate the derivative of Psi
  -my.Psi(x, my.pi) * (x + my.Psi(x, my.pi))
} # end my.Psi.dash


phi = function(x){dnorm(x)}


rnorm.trunc <- function (n, mu, sigma, lo=-Inf, hi=Inf)
{
  # Calculate truncated normal
  p.lo <- pnorm (lo, mu, sigma)
  p.hi <- pnorm (hi, mu, sigma)
  u <- runif (n, p.lo, p.hi)
  
  return (qnorm (u, mu, sigma))
} # end rnorm.trunc


# MBfilter.dialog = list( title='Model-based Filtering',
#   m.dataframeItem=NULL, label='Data to filter',
#   signal = c("default", "get.dataset.factors", "treatment"),
#   signal = c("default", "get.dataset.row.metadata.fields", "protein_group"),    
#   treatment.choiceItem=NULL, label='Factors',
#   protein_group.choiceItem = NULL, label = "Field Containing Proteins",   
#   compute_pi.trueFalseItem=FALSE, label='Manually Estimate PI',
#    tooltip = "If this is checked, the user can set the estimated random missingness probability",
#   signal = c("default", "toggle.sensitive", "my.pi"),
#     my.pi.numericItem="0.05", label='PI', indent=10
# )
  
  
  
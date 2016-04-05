#"Improved quality control processing of peptide-centric LC-MS proteomics data"
#Melissa M. Matzke, Katrina M. Waters, Thomas O. Metz1, Jon M. Jacobs1,
#Amy C. Sims2, Ralph S. Baric2, Joel G. Pounds2 and Bobbie-Jo M. Webb-Robertson


#data PXN matrix peptide, Samples,
#factors, mapping of N columns to factors
#return variable containing full correlation matrix and average correlations
CorrelationMatrix <- function(data, classArray) 
{
    nPeps <- dim(data)[1] #Get number of peptides
    nSamples <- dim(data)[2] #Get total number of Samples
    CM <- matrix(1, nrow = nSamples, ncol = nSamples) 
    #build N x N correlation matrix
    for(i in 1: (nSamples-1))
    {
        for( j in (i+1) : nSamples)
        {
            nanCov <- cov((cbind(data[,i],data[,j])), use = "complete.obs")
            CM[i,j] = nanCov[1,2]/sqrt(nanCov[1,1]*nanCov[2,2])
            CM[j,i] = CM[i,j]
        }
    }
    
    CM_ClassArray <- CM

    for(j in 1:nSamples)
    {
        CM_ClassArray[j,j] = NA
    }    
    CM_ClassCurrent = matrix(1, c(nSamples, 1))
    for(k in 1:nSamples)
    {
        a <- which(classArray == classArray[k])
        CM_ClassCurrent[k] = mean(CM_ClassArray[k,a], na.rm = TRUE)
    }
    
    return(list(FullMatrix = as.matrix(CM),BioCorr = as.matrix(CM_ClassCurrent)))
}
  
#provides the skew, a measure of the asymmetry of a distribution
GetSkew <- function(data)
{
    require(moments)      #load moments library
    nSamples <- dim(data)[2] #Get total number of Samples
    skewMatrix  <-  matrix(1, nrow = nSamples, ncol = 1)
    for(i in 1:nSamples)
    {
        dlen = length(data[!is.na(data[,i]),i])
        skewMatrix[i] =  (dlen*(dlen-1))^(1/2) / (dlen - 2) * skewness(data[,i], na.rm = TRUE) 
    }
    return(skewMatrix)
}


GetKurtosis <- function(data)
{
    require(moments)
    nSamples <- dim(data)[2] #Get total number of Samples
    kurtosisMatrix  <-  matrix(1, nrow = nSamples, ncol = 1)
    for(i in 1:nSamples)
    {
        dlen = length(data[!is.na(data[,i]),i])
        kurtosisMatrix[i] = (dlen - 1) /((dlen-2) * (dlen-3)) *((dlen+1)* kurtosis(data[,i],na.rm = TRUE) - 3*(dlen-1))
    }
    return(kurtosisMatrix)
}

# data PXN matrix peptide, Samples
GetMissingness <- function(data)
{
    nPeps <- dim(data)[1] #Get number of peptides
    nSamples <- dim(data)[2] #Get total number of Samples
    missingnessMatrix  <-  matrix(1, nrow = nSamples, ncol = 1)
    for(i in 1:nSamples)
    {
        missingnessMatrix[i] = length(which(is.na(data[,i]))) / nPeps
    }
    return(missingnessMatrix)
}

#Gets the median absolute deviation for each sample
GetMAD <- function(data)
{
    nSamples <- dim(data)[2] #Get total number of Samples
    madMatrix  <-  matrix(1, nrow = nSamples, ncol = 1)
    for(i in 1:nSamples)
    {
        madMatrix[i] = mad(data[,i], na.rm = TRUE, constant = 1)
    }
    return(madMatrix)
}

#   ROBUST PRINCIPAL COMPONENTS ANALYSIS BASED ON PROJECTION PURSUIT
#
# Croux and Ruiz-Gazen (2005)
# "High Breakdown estimators for Principal Components: the
# Projection-Pursuit Approach Revisited", Journal of Multivariate Analysis,
# 95, 206-226.
#x data-matrix, pp number of desired vectors
RobustPCA <- function(x,pp)
{
    n = nrow(x)
    p = ncol(x)
    if (pp > min(n, p))
    {
        stop("pp too large")
    }
    if (p>n)
    {
        svdx = svd(t(x))
        x = svdx$u %*% svdx$d
        pold = p
        p = n
    }
    else
    {
        pold = p
    }
    
    m = l1median_NLM(x)$par
        
    y <-  x - kronecker(matrix(1,n,1), t(m))     #Center data using L1 median value
    bigscores <- matrix(0,n,pp)
    
    veig = matrix(0,pp,0)
    lambda = matrix(0,0,1)
    
    for (k in 1:pp)
    {
        if (k < p)
        {
            pcol = matrix(0,n,1)
            for(i in 1:n)
            {
                pyi = y[i,]
                pyi = t(rbind(pyi))
                npyi=sum(abs(pyi)^2)^(1/2)
                if (npyi==0)
                {
                    pcol[i]=0
                }
                else
                { 
                    pyi=pyi/npyi
                    pcol[i]=mad(y %*% pyi, na.rm = TRUE)
                }
            }
            lambdastar = max(pcol)
            istar = which(pcol == lambdastar)
            lambda = rbind(lambda, lambdastar)
            vhelp = t(rbind(y[istar,]))
            vhelp = vhelp / sum(abs(vhelp)^2)^(1/2)
            scores = y %*% vhelp
            y = y - (scores %*% t(vhelp))
        }
        else
        {           # last eigenvector is automatically found
            i = 1
            while(sum(abs(y[i,])^2)^(1/2) == 0)
            {
                i=i+1
            }
            vhelp=t(rbind(y[i,]))
            vhelp=vhelp / sum(abs(vhelp)^2)^(1/2)
            scores= y %*% vhelp
            lambda=rbind(lambda, mad(y %*% vhelp))
        }
        veig= cbind(veig, vhelp)
        bigscores[,k]=scores
        k=k+1
    }
    
    if (pold>n)
    {
        veig = svdx$v %*% veig
    }
    
    lambda=lambda^2
    scores=bigscores

    return(list(Lambda = as.matrix(lambda), Veig = as.matrix(veig), Scores = as.matrix(scores)))

}

#Majority of the processing occurs here.
#data = sampleMatrix
#class = vector of factors to compare
#techreps = vector of technical replicates
DetectOutliers <- function(data, class, techreps, pvalue_threshold = 0.0001)
{
    

    #data<-myData
    #data <- log10(data)
    #class <- c(1,1,1,2,2,2,3,3,3)
    #techreps  <-  c(1,1,1,2,2,2,3,3,3)

    
    nPeps <- dim(data)[1] #Get number of peptides
    nSamples <- dim(data)[2] #Get total number of Samples
    CM <- matrix(1, nrow = nSamples, ncol = nSamples)
        

    
    corrData <- CorrelationMatrix(data,class)
    a <- GetMissingness(data)
    b <- GetMAD(data)        
    c <- GetKurtosis(data)
    d <- GetSkew(data)
    e <- corrData$BioCorr
    

    f <- corrData$FullMatrix

    QualityMatrix <- cbind(a,b,c,d,e)
    
    q = dim(QualityMatrix)[2]
    #Robust PCA
    pc <- RobustPCA(QualityMatrix, q)

    pcrow = dim(pc$loadings)[1]
    pccol = dim(pc$loadings)[2]
            


    eigvec <- cbind(pc$Veig)
    eigvec_t <- t(eigvec)
    
    lambda <- cbind(as.matrix(pc$Lambda,row.names)[,1])
    explained_var <- lambda/sum(lambda)
#    lambda <- lambda/sum(lambda)
    
    #Robust Covariance Estimate
    C_Sn <- matrix(0, nrow = q, ncol = q)
    
    for(i in 1:q)
    {
        C_Sn = C_Sn +  lambda[i] * cbind(eigvec[,i]) %*% rbind(eigvec_t[i,])
    }
    
    QM_Median <- matrix(0, nrow = 1, ncol = q)
    for(i in 1:q)        {
        QM_Median[1,i] = median(QualityMatrix[,i])
    }
    
    dM_r = matrix(NA, nrow = q, ncol = 1)
    
    for(i in 1: (length(class)))
    {
        dM_r[i] = (rbind(QualityMatrix[i,]) - QM_Median) %*% solve(C_Sn) %*% t(rbind(QualityMatrix[i,]) - QM_Median)
    }
    
    LOG2RMD <- log(cbind(dM_r),base = 2)
    
    RMD_PVALUES <- 1 - pchisq(dM_r, q)
    RMD_PVALUES <- cbind(RMD_PVALUES)
    
    suspect_runs = cbind(which(RMD_PVALUES <= pvalue_threshold))
    suspect_runs_DM = t(dM_r[suspect_runs])
    

    
    keep_runs = cbind(which(RMD_PVALUES > pvalue_threshold))
    keep_runs_dM = t(dM_r[keep_runs])
    
    Outlier_Runs = suspect_runs;
    Outlier_Samples_IDX = matrix(0, nrow = 0, ncol = 1) 
    Outlier_Tech_rep_IDX = matrix(0, nrow = 0, ncol = 1)

    n = length(techreps)
    ns = max(techreps)
    
    notc = matrix(0, n,1)
    notc[Outlier_Runs] = 1
    notc = notc * techreps
    
    for(i in 1:ns)
    {
        count1 = length(which(techreps == i))
        count2 = length(which(notc == i))
        if(count2 > 0)
        {
            if(count1 == count2)
            {
                Outlier_Samples_IDX = rbind(Outlier_Samples_IDX, cbind(which(notc == i)))
            }
            else
            {
                Outlier_Tech_rep_IDX = rbind(Outlier_Tech_rep_IDX, cbind(which(notc == i)))
            }
        }   
    }
    outlier_sample_dM = t(dM_r[Outlier_Samples_IDX])
    outlier_tech_reps = t(dM_r[Outlier_Tech_rep_IDX])

    #Keep_runs is a vector of numbers referring to jobs (1...n)
    return(list(Keep_runs = keep_runs, Keep_runs_dM = keep_runs_dM, DM_r = dM_r, 
                Outlier_Samples_IDX = Outlier_Samples_IDX, 
                Outlier_Tech_rep_IDX = Outlier_Tech_rep_IDX, 
                CorrelationMatrix = f, TechReps = techreps, Data = data, 
                Outlier_sample_dM = outlier_sample_dM, Outlier_tech_reps = outlier_tech_reps))   
}


    
GetBoxPlot <- function(outlierResults)
{
    mycolors = c("red", "yellow", "green", "blue", "lightblue", "pink", "orange")
    boxplot(outlierResults$Data,col = mycolors[class %% 7 + 1], main ="Peptide Abundance Distribution by LC Analysis",
            cex = 0.6, notch = TRUE,
            names = seq(1:dim(outlierResults$Data)[2]))
}

GetCorrHeatMap <- function(outlierResults)
{
     heatmap.2(outlierResults$CorrelationMatrix, 
               Rowv = NA, Colv = NA, dendrogram = "none", 
               trace = "none", linecol = "blue", 
               main = "Correlation Heat Map")
}

Get_dMr_Plot <- function(outlierResults)
{
        dM_rplot = log(outlierResults$DM_r, base = 2)

        plot(outlierResults$Keep_runs, log(outlierResults$Keep_runs_dM, base = 2), ylim = c(min(dM_rplot) - 1,
             max(dM_rplot) + 1), 
             col = "gray", 
             pch = 17, 
             xlim = c(-2, length(outlierResults$TechReps)+2), 
             ylab = expression(paste("Log"[2],"(Mahalanobis Distance)")),
             xlab = "Sample Number",
             main = "Robust Mahalanobis Distance")
        

        #plot positions
        library(fields)
        library(geoR)
        yline(log2(25.744831), col = "red") #will not always work need to account for different q? maybe not will most often be 5


        
        points(outlierResults$Outlier_Samples_IDX, log2(outlierResults$Outlier_sample_dM), col ="red", pch = 17)
        points(outlierResults$Outlier_Tech_rep_IDX, log2(outlierResults$Outlier_tech_reps), col = "blue", pch = 17)

        text(outlierResults$Outlier_Samples_IDX, log2(outlierResults$Outlier_sample_dM), 
             outlierResults$Outlier_Samples_IDX, cex = 0.6, pos = 4)
        text(outlierResults$Outlier_Tech_rep_IDX, log2(outlierResults$Outlier_tech_reps), 
             outlierResults$Outlier_Tech_rep_IDX, cex = 0.6, pos = 4)
}



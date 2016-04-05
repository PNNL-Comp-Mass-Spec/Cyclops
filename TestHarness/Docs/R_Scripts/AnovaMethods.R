
# Written by Ashoka D. Polpitiya
# for the Department of Energy (PNNL, Richland, WA)
# Copyright 2007, Battelle Memorial Institute
# E-mail: ashoka.polpitiya@pnl.gov
# Website: http://omics.pnl.gov/software
# -------------------------------------------------------------------------
#
# Licensed under the Apache License, Version 2.0; you may not use this file except
# in compliance with the License.  You may obtain a copy of the License at
# http://www.apache.org/licenses/LICENSE-2.0
#
# ANOVA modified by Joseph N. Brown, Ph.D.

performAnova <- function(Data, FixedEffects,
                    RandomEffects, thres=3,
                    interact=FALSE,
                    unbalanced = TRUE,
                    useREML=TRUE,
                    Factors=factors)
{
    require(car)
    require(nlme)

    Data <- Data[order(as.numeric(rownames(Data))),]
    allFactors <- c(FixedEffects,RandomEffects)
    splitIdx <- splitForAnova(Data, Factors[allFactors,], thres=thres)
    Data.good <- Data[splitIdx$Good,,drop=FALSE]
    Data.bad <- Data[splitIdx$Bad,,drop=FALSE]
    if (dim(Data.bad)[1]==1)
      Data.bad <- rbind(Data.bad, Data.bad)
        
    #test for the length of p-value vector 
    tmp <- retrievePvals(runif(dim(Data.good)[2]), FixedEffects, 
	 	                    RandomEffects, Factors=Factors, interact=interact, 
	 	                    unbalanced=unbalanced, test=TRUE, useREML=useREML) 
    pNames <- names(tmp) 
	 	 
    anovaresults <- t(apply(Data.good, 1, retrievePvals, FixedEffects,
                    RandomEffects, Factors=Factors, interact=interact,
                    unbalanced=unbalanced, Np=length(tmp),
                    test=FALSE, useREML=useREML))

	
    if (dim(anovaresults)[1] < dim(anovaresults)[2])
        anovaresults <- t(anovaresults)
    colnames(anovaresults)<-pNames
	
	out <- c()
    if (is.matrix(anovaresults))
    {
        out <- anovaresults
        outColNames <- pNames
        for (i in 1:length(pNames))
        {
            idx <- !is.na(anovaresults[,i])
            bhval <- rep(NA, length(idx))
			
            tryCatch(
            {
				bhval <- p.adjust(anovaresults[,i], method="BH")
            },
            interrupt = function(ex)
            {
                cat("An interrupt was detected.\n");
                print(ex);
            },
            error = function(ex)
            {
                cat("An error was detected.\n");
                print(ex);
            },
            finally =
            {
				out <- cbind(out, bhval)
				outColNames <- c(outColNames, paste("AdjPval_", pNames[i], sep=""))
            }) # tryCatch()
        }
        colnames(out) <- outColNames
	}
	
	out <- merge(x=out, y=Data, by="row.names", all=T)
	rownames(out) <- out[,1]
	out <- out[,-1]
	out <- out[with(out, order(out[,2])),]
	
    return(list(pvals=out, miss=Data.bad, allused=(dim(Data.bad)[1]==0)))
}

#--------------------------------------------------------------
retrievePvals <- function(x, fEff, rEff,
                   Factors=factors,
                   interact=FALSE,
                   unbalanced=TRUE,
                   Np=2,
                   test=FALSE,
                   useREML=TRUE)
{
    allF <- c(fEff,rEff)
    X <- data.frame(t(Factors[allF, , drop=FALSE]), x)

    for (i in 1:(dim(X)[2]-1))
    {
        names(X)[i] <- allF[i]
    }
	
    lhs <- fEff[1]
    if (length(fEff) > 1)
    {
        for (i in 2:length(fEff))
        {
            if (interact)
                lhs <- paste(lhs, "*", fEff[i])
            else
                lhs <- paste(lhs, "+", fEff[i])
        }
    }
    lm.Formula <- as.formula(paste('x~', lhs))
    modelF <- lm(lm.Formula, X)
		
    if (useREML)
        Method <- "REML"
    else
        Method <- "ML"
    	
    if (!is.null(rEff))
    {
        rEffects <- paste("~1|", rEff[1], sep="")
        if (length(rEff) > 1)
        {
            for (i in 2:length(rEff))
            {
                rEffects <- paste(rEffects, ", ~1|", rEff[i])
            }
            rEffects <- paste("random=list(", rEffects, ")", sep="")
        }
        else
            rEffects <- paste("random=",rEffects, sep="")

        modelR <- lme(lm.Formula, data=X, eval(parse(text=rEffects)),
                      method=Method, na.action = na.omit)
		

        options(warn = -1)
        if (unbalanced)
            anova.result <- try(anova(modelR, type="marginal"),silent=TRUE)
        else
            anova.result <- try(anova(modelR),silent=TRUE)

        options(warn = 0)
        
        if(inherits(anova.result, "try-error"))
        {
            return(rep(NA, Np))
        }
        else
        {
            pvals <- anova.result$"p-value"
            names(pvals)<-rownames(anova.result)

            if (names(pvals)[1] == "(Intercept)")
                    pvals <- pvals[-1]
            idx <- length(names(pvals))
            if (names(pvals)[idx] == "Residuals")
                    pvals <- pvals[-idx]

            pvals[is.nan(pvals)] <- NA
            if (test)
                return(pvals)
            else
            {
                tmp <- rep(NA, Np)
                tmp[1:length(pvals)] <- pvals
                return(tmp)
            }
        }
    }
    else  # No random effects
    {
        if (unbalanced)
            anova.result <- try(Anova(modelF, type="III"),silent=TRUE)
        else
            anova.result <- try(Anova(modelF),silent=TRUE)

        if(inherits(anova.result, "try-error"))
        {
            return(rep(NA, Np))
        }
        else
        {
            pvals <- anova.result["Pr(>F)"][[1]]
            names(pvals)<-rownames(anova.result)

            if (names(pvals)[1] == "(Intercept)")
                    pvals <- pvals[-1]
            idx <- length(names(pvals))
            if (names(pvals)[idx] == "Residuals")
                    pvals <- pvals[-idx]

            if (test)
                return(pvals)
            else
            {
                tmp <- rep(NA, Np)
                tmp[1:length(pvals)] <- pvals
                return(tmp)
            }
        }
    }
}

#--------------------------------------------------------------
splitForAnova <- function(Data,Factors,thres=3)
{
    anovaIdx <- integer(0)
    anovaIdxNon <- integer(0)
    allIdx <- 1:dim(Data)[1]

    if (is.matrix(Factors))
    {
        N <- dim(Factors)[1]  # how many factors?
    }
    else
        N <- 1
    for (k in 1:N)
    {
        if (N > 1)
            currFac <- Factors[k,]
        else
            currFac <- Factors

        splitIdx <- splitmissing_factor(Data, currFac, thres=thres)
        if (k == 1)
            anovaIdx <- splitIdx$good
        else
            anovaIdx <- intersect(anovaIdx, splitIdx$good)
    }
    anovaIdxNon <- allIdx[!(allIdx %in% anovaIdx)]
    out <- list(Good=anovaIdx,Bad=anovaIdxNon)
    return(out)
}

#--------------------------------------------------------------
factor_values <- function(factors)
{
    out <- list()
    for (i in 1:dim(factors)[1])
    {
        out[[rownames(factors)[i]]] <- as.matrix(factors[i,!duplicated(t(factors[i,]))])
    }
    return(out)
}

#--------------------------------------------------------------
splitmissing_factor <- function(Data, Factor, thres=3)
{
    anovaIdx <- integer(0)
    anovaIdxNon <- integer(0)
    allIdx <- 1:dim(Data)[1]
    Nreps <- unique(as.vector(t(Factor))) # Factor Levels
    for (i in 1:length(Nreps)) # for each unique factor level
    {
        idx <- which(Factor == Nreps[i])
        dataset <- Data[,idx,drop=FALSE]
        if (length(idx) > 1) # multiple columns
        {
            splitIdx <- splitmissing_fLevel(dataset, thres=thres)
            if (i == 1)
                anovaIdx <- splitIdx$good
            else
                anovaIdx <- intersect(anovaIdx, splitIdx$good)
        }
    }
    badIdx <- allIdx[!(allIdx %in% anovaIdx)]
    return(list(good=anovaIdx, bad=badIdx))
}

#--------------------------------------------------------------
splitmissing_fLevel <- function(Data,thres=3)
{
    allIdx <- 1:dim(Data)[1]
    validIdx <- rowSums(!is.na(Data)) >= thres
    #validIdx <- apply(!is.na(Data),1,sum) >= thres

    goodIdx <- unique(which(validIdx))
    badIdx <- allIdx[!(allIdx %in% goodIdx)]
    return(list(good=goodIdx, bad=badIdx))
}

#--------------------------------------------------------------
jnb_trim <- function(x) {
	if (length(x) > 0) {
		t <- c()
		for (i in 1:length(x)) {
			if (nchar(as.character(x[i])) > 0)
				t <- c(t, x[i])
		}	
		return(t)
	}
}


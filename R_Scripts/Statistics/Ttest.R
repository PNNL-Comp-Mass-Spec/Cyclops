
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
# One Sample t-test

Ttest <- function(Data, thres=3)
{
    require(qvalue)
    require(car)
    require(nlme)

    Data <- Data[order(as.numeric(rownames(Data))),]
    splitIdx <- splitmissing.fLevel(Data, thres=thres)
    Data.good <- Data[splitIdx$good,,drop=FALSE]
    Data.bad <- Data[splitIdx$bad,,drop=FALSE]
    if (dim(Data.bad)[1]==1)
      Data.bad <- rbind(Data.bad, Data.bad) # trick to make it a two row matrix

    browser()
    Tresults <- t(t(apply(Data.good, 1, OneSampleTtest)))

    idx <- !is.na(Tresults)
    qval <- rep(NA, length(idx))

    out <- Tresults
    outColNames <- "p-value"
    
    tryCatch(
    {
        qval.tmp <- (qvalue(Tresults[idx]))$qvalues
        qval[idx] <- qval.tmp
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
        out <- cbind(out,qval)
        outColNames <- c(outColNames, "q-value")
    }) # tryCatch()
    colnames(out) <- outColNames

    return(list(pvals=out, miss=Data.bad, allused=(dim(Data.bad)[1]==0)))
}

#--------------------------------------------------------------
OneSampleTtest <- function(x)
{
    ttest <- t.test(x)
    return(ttest$p.value)
}





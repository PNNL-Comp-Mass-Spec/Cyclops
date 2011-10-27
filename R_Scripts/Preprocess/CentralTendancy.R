
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
# Central Tendancy Adjustment

MeanCenter.Sub <- function(Data, Mean=TRUE, centerZero=TRUE)
{
    ndata <- Data

    if (Mean)
        Center <- apply(Data,2,mean,na.rm=TRUE)
    else
        Center <- apply(Data,2,median,na.rm=TRUE)

    centerM <- matrix(Center,nrow=dim(Data)[1],ncol=dim(Data)[2],
                        byrow=TRUE)
    if (centerZero)
        ndata <- Data - centerM
    else
    {
        newAverage <- max(Center,na.rm=TRUE)
        ndata <- Data - centerM + newAverage
    }
    return(ndata)
}

######################## Mean center the data ###############
MeanCenter.Div <- function(Data, Mean=TRUE, centerZero=TRUE)
{
    ndata <- Data

    if (Mean)
        Center <- apply(Data,2,mean,na.rm=TRUE)
    else
        Center <- apply(Data,2,median,na.rm=TRUE)

    centerM <- matrix(Center,nrow=dim(Data)[1],ncol=dim(Data)[2],
                        byrow=TRUE)
    if (centerZero)
        ndata <- Data / centerM
    else
    {
        newAverage <- max(Center,na.rm=TRUE)
        ndata <- (Data / centerM) * newAverage
    }
    return(ndata)
}

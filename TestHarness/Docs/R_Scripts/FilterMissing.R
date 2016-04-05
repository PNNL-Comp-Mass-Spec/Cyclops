
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
# Filter data based on a cutoff for missing values


filterMissing <- function(Data,cutoff=20)
{
    cutoff <- cutoff/100
    allmissIdx <- apply(is.na(Data),1,sum) == dim(Data)[2]
    missingData <- Data[allmissIdx,,drop=FALSE]
    okData <- Data[!allmissIdx,]
    index <- apply(okData, 1, function(x,threshold) ((sum(is.na(x))/length(x)) > threshold),
                threshold=cutoff)
    out <- okData[!index, ,drop=FALSE]
    return(out)
}

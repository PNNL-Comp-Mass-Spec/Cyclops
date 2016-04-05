
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
# R Plotting functions used in DAnTE
# -------------------------------------------------------------------------

Boxplots <- function(x,
                         Columns = NULL,
                         file="deleteme.png",
                         colorByFactor = FALSE,
						 colorFactorTable = NULL,                                       
						 colorFactorName = NULL,
                         outliers=TRUE,
                         color="wheat2",
                         bkground="white",
                         labelscale=0.8,
                         boxwidth=1,
                         showcount=TRUE,
                         showlegend = TRUE,
                         stamp=NULL,
                         do.ylim=FALSE,
                         ymin=NULL,
                         ymax = NULL,
						 ylabel="Relative Abundance",
				IMGwidth=1200,
				IMGheight=1200,
				FNTsize=12,
				res=600,
                         ...)
{ 
  require(Cairo)
  CairoPNG(filename=file,width=IMGwidth,height=IMGheight,pointsize=FNTsize,bg=bkground,res=res) 

  #if user does not specify columns, just take all of them
  if(is.null(Columns) || Columns == "") { Columns=colnames(x) }
  par(oma=c(3.4, 2, 2, 2), mar=c(5,4,4,1))
  
  box_color <- rep(color, dim(x)[2])
  
  # Prepare the colorFactor
  colorFactor <- c()
  if (colorByFactor && !is.null(colorFactorTable) && !is.null(colorFactorName)) {
	ColMeta <- unique(subset(colorFactorTable, select=c("Alias", colorFactorName)))
	
	if (length(ColMeta$Alias) == length(colnames(x))) {
		ColMeta <- ColMeta[which(ColMeta[,"Alias"] %in% colnames(x)),]
	}
	else if (length(unique(ColMeta[,colorFactorName])) == length(colnames(x))) {
		ColMeta <- unique(as.data.frame(ColMeta[,colorFactorName]))	
		colnames(ColMeta) <- "Alias"
		ColMeta <- as.data.frame(ColMeta[which(ColMeta[,"Alias"] %in% colnames(x)),])
		colnames(ColMeta) <- "Alias"
		ColMeta <- cbind(ColMeta, ColMeta)
		colnames(ColMeta) <- c("Alias", colorFactorName)
	}
	
	coln <- data.frame("Alias" = colnames(x))
	mdata <- merge(x=coln, y=ColMeta, by.x="Alias", by.y="Alias", all.y=F)
	colorFactor = unlist(subset(mdata, select=colorFactorName))
  }
  else {
	do.ylim= TRUE
  }
  
  uF <- c()
  if (colorByFactor && length(colorFactor) == dim(x)[2])
      {
        uF <- unique(colorFactor)
        colStep <- length(uF)
        colorRange <- hsv(h = seq(0,1,1/colStep), s=1, v=1)
        for (i in 1:length(uF))
        {
            idx <- which(uF[i]==colorFactor)
            box_color[idx] <- colorRange[i]
        }
      }
    

  x <- x[,Columns, drop=F]
  
      par(omd=c(0,1,0.1,1))


      if(do.ylim)
        boxplot(data.frame(x),outline=outliers,notch=T,las=2,
            boxwex=boxwidth,col=color,cex.axis=labelscale,ylim=c(ymin, ymax),
			ylab=ylabel,...)
      else 
        boxplot(data.frame(x),outline=outliers,notch=T,las=2,
          boxwex=boxwidth,col=box_color,cex.axis=labelscale,ylab=ylabel,...)      
      if (showlegend && colorByFactor && length(colorFactor) > 0) {
        legend("topleft", "(x,y)", uF,col=colorRange[1:length(uF)],pch=19,
            bg='transparent')
        }
      if (showcount)
      {
        axis(side=3, at=1:dim(x)[2], labels=colSums(!is.na(x)), tick=FALSE,
            cex.axis=.7, las=2)
      }

  dev.off()
}
# Written by Joseph N. Brown
# for the Department of Energy (PNNL, Richland, WA)
# Battelle Memorial Institute
# E-mail: joseph.brown@pnnl.gov
# Website: http://omics.pnl.gov/software
# -----------------------------------------------------
#
# Notice: This computer software was prepared by Battelle Memorial Institute,
# hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the
# Department of Energy (DOE).  All rights in the computer software are reserved
# by DOE on behalf of the United States Government and the Contractor as
# provided in the Contract.
#
# NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY WARRANTY, EXPRESS OR
# IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS SOFTWARE.
#
# This notice including this sentence must appear on any copies of this computer
# software.
# -----------------------------------------------------#/

################################################################################
## FUNCTIONS
################################################################################

#######################################################################################
# Performs Beta-Binomial and QuasiTel analyses on Spectral Count data and combines results
#######################################################################################
#	Packages Required: BetaBinomial, QuasiTel
#	Parameters:
#	tData: Spectral Count crosstab (must contain 0 for missing values)
#	colMetadata: name of T_Column_Metadata table
#	colFactor: column name (in colMetadata) mapping to the factor to be tested
#	sinkFileName: file name to save Beta-Binomial results to (tab-delimited), leave as NULL if no output should be made.
#
#	Returns:
#	data.frame containing the combined results of the Beta-Binomial and QuasiTel analyses
#######################################################################################
jnb_BBM_and_QTel <- function(
  tData, colMetadata, colFactor,
  theta=TRUE,
  sinkFileName=NULL)
{
  require(BetaBinomial)
  
  tData[is.na(tData)] <- 0
  if (!is.null(sinkFileName))
    sink(sinkFileName)
  bb_Pvalues <- largescale.bb.test(x=tData, colMetadata[,colFactor], theta=theta)
  rownames(bb_Pvalues) <- rownames(tData)
  colnames(bb_Pvalues) <- 'BBM_Pvals'
  if (!is.null(sinkFileName))
    sink()
  
  bb_Pvalues <- cbind(bb_Pvalues, 
	'BBM_AdjPvals'=p.adjust(bb_Pvalues[,'BBM_Pvals'], method='BH'))
	
  qTel_Res <- c()
  fac <- colMetadata[,colFactor]
  uF <- unique(fac)
  for (i in 1:length(uF))
  {
    for (j in 2:length(uF))
    {
      if (i < j)
      {		
        td <- cbind(tData[,which(fac==uF[i])], 
                    tData[,which(fac==uF[j])])
        colnames(td)= c(
			rep(as.character(uF[i]), length(fac[fac==uF[i]])),
			rep(as.character(uF[j]), length(fac[fac==uF[j]])))
			
        t <- quasitel(data=td, group1=as.character(uF[i]), group2=as.character(uF[j]))
        colnames(t) <- paste(as.character(uF[i]), '_v_', as.character(uF[j]), '_', 
                             colnames(t), sep='')
        
        qTel_Res <- cbind(qTel_Res, t)
      }
    }
  }
  
  finalRes <- cbind(bb_Pvalues, qTel_Res)
  return(finalRes)
}
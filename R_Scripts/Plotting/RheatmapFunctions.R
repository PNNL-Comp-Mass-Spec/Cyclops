
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
## HEATMAP PARAMETERS
################################################################################

################################################################################
## HEATMAP FUNCTIONS
################################################################################
jnb_GetHeatmapMatrix <- function(hm) {
  tmp <- t(hm$carpet)
  tmp <- tmp[ nrow(tmp):1,]
	return(tmp)
}

IntraIndividualZscore <- function(ds, Separator)
{
	# uses the same separator you pass in for heatmap.2
	Sep <- c(1, Separator, ncol(ds))
	tmp <- matrix()
	# print(Sep)
	for (i in 1:(length(Sep) - 1))
	{
		# print(paste(i,":",Sep[i], "-", Sep[i+1]))
		#
		 if (i == 1)	{
				#tmp <- Zscore(df, cols[c(Sep[i]:Sep[i+1])])
				cols <- c(Sep[i]:Sep[i+1])
				# print(cols)
				tmp <- Zscore(ds, cols)
		 } else 
		 if (i > 1) {
			num1 <- Sep[i] + 1
			num2 <- Sep[i + 1]
			# print(c(num1:num2))
			cols <- c(num1:num2)
			tmp <- cbind(tmp, Zscore(ds, cols))
		 }
	}
	return(tmp)
}

hclust2 <- function(x, method="complete",...)
	hclust(x, method=method,...)
dist2 <- function(x, ...)
	as.dist(1-cor(t(x), method="pearson"))


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

# Returns a data.frame obtained by aggregating via a specified function across
# columns or rows
# x is the data.frame or matrix
# myFactor is the factor that you want to aggregate to
# MARGIN is a vector giving the subscripts which the function will be applied over
#  e.g. 1 indicates rows, 2 indicates columns
# FUN is the aggregation function
jnb_Aggregate <- function(x, myFactor, MARGIN, FUN)
{
	if (MARGIN == 1)
	{
		# test the number of rows and factors passed
		if (length(myFactor) != nrow(x))
			stop(paste("The number of factors passed",
			"does not equal the number of rows in your data"))

		tmp <- by(x, INDICES=myFactor, FUN=FUN)
		return(as.data.frame(do.call(rbind, tmp)))
	} else if (MARGIN == 2) {
		# test the number of columns and factors passed
		if (length(myFactor) != ncol(x))
			stop(paste("The number of factors passed",
			"does not equal the number of columns in your data"))

		tmp <- by(t(x), INDICES=myFactor, FUN=FUN)
		return(as.data.frame(do.call(cbind,tmp)))
	}
}
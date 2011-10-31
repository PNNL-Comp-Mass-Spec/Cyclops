
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

# Function to append an item to a list
lappend <- function(lst, obj) {
    lst[[length(lst)+1]] <- obj
    return(lst)
}

# Function to determine if a package is installed
jnbIsPackageInstalled <- function(Package) {
	a <- installed.packages()
	packages <- a[,1]
	is.element(Package, packages)
}

# Calculate a Log2 Ratio
jnb_Log2Ratio <- function(x1, x2) {
	if ( (x1==0 & x2==0) | (is.na(x1) & is.na(x2)) | (is.na(x1) & x2==0) | (x1==0 & is.na(x2)) ) {
		return(0)
	} else if (x2==0 | is.na(x2) ) {
		return(log(x1,2))
	} else if (x1==0 | is.na(x1) ) {
		return (-log(x2,2))
	} else {
		return(log(x1/x2,2))
	}
}

# Processes log2 fold-change on a dataframe or matrix, and takes into account columns specified for p-values
jnb_FoldChangeSpectralCountAndPackage <- function(
		x, 				# Data frame or data matrix
		pValueColumn) # Column(s) representing p-values
{
	Pval_tmp = c()
	if (length(pValueColumn) > 0 & pValueColumn != 0)
	{
		#expects P-values in the first column
		Pval_tmp <- x[,pValueColumn]
		x <- x[,-pValueColumn]
	}
	
	header <- c()
	FC_tmp <- c()
	for (i in 2:ncol(x)-1)
	{
		for (j in (i+1):ncol(x))
		{
			tmp1 <- paste(colnames(x)[i], "_v_", colnames(x)[j],sep="")
			header <- c(header, paste(colnames(x)[i], "_v_", colnames(x)[j],sep=""))
			FC_tmp <- cbind(FC_tmp, mapply(FUN=jnb_Log2Ratio, x1=x[,i], x2=x[,j]))
		}
	}
	colnames(FC_tmp) <- header
	
	if (length(pValueColumn) > 0 & pValueColumn != 0) {
		FC_tmp <- cbind("Pvalue"=Pval_tmp, FC_tmp, x)
	} else {
		FC_tmp <- cbind(FC_tmp, x)
	}
}
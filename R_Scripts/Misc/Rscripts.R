
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
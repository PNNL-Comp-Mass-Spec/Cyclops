
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

# Provides a number of different spectral count algorithms, including
# Total Signal (type = 1)
# Z-normalization (type = 2)
# Natural Log Preprocessing (type = 3)
# Hybrid (total signal followed by Z-normalization) (type = 4)
jnb_NormalizeSpectralCounts <- function(x, type) {
	x <- data.matrix(x)
	x[is.na(x)] <- 0
	
	if (type == 1) { 				# Total Signal
		s <- colSums(x)
		w <- c()
		for (i in 1:nrow(x)) {
			w <- rbind(w, x[i,]/s)
		}
		return(w)
	} else if (type == 2) {			# Z-normalization
		z <- t(scale(t(x)))
		return(z)
	} else if (type == 3) {			# Log Preprocessing
		l <- log(x)
		l[l == -Inf] <- 0
		return(l)
	} else if (type == 4) {			# Hybrid Normalization (TS, Z)
		s <- colSums(x)
		w <- c()
		for (i in 1:nrow(x)) {
			w <- rbind(w, x[i,]/s)
		}
		h <- t(scale(t(w)))
		return(h)
	}
}
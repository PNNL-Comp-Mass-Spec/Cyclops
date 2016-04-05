
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
# Function to append an item to a list
#######################################################################################
lappend <- function(lst, obj) {
    lst[[length(lst)+1]] <- obj
    return(lst)
}

#######################################################################################
# Function to determine if a package is installed
#######################################################################################
jnbIsPackageInstalled <- function(Package) {
	a <- installed.packages()
	packages <- a[,1]
	is.element(Package, packages)
}

#######################################################################################
# Calculate a Log2 Ratio
#######################################################################################
jnb_Log2Ratio <- function(x1, x2) {
  if ( (x1==0 & x2==0) | (is.na(x1) & is.na(x2)) | (is.na(x1) & x2==0) | (x1==0 & is.na(x2)) ) {
  	return(0)
  } else if (x2==0 | is.na(x2) ) {
    if (x1 < 0) {
      return(-log(abs(x1),2))
    } else {    
		  return(log(x1,2))
    }
  } else if (x1==0 | is.na(x1) ) {
    if (x2 < 0) {
      return(log(abs(x2),2))
    } else {
		  return (-log(x2,2))
    }
  } else {
    if (x1 < 0 & x2 < 0) {
      return(log(abs(x1)/abs(x2), 2))
    } else if (x1 < 0) {
      return(-log(abs(x1)/x2,2))
    } else if (x2 < 0) {
      return(log(x1/abs(x2),2))
    } else {
		  return(log(x1/x2,2))
    }
  }
}

#######################################################################################
# Processes log2 fold-change on a dataframe or matrix, and takes into account columns specified for p-values
#######################################################################################
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

# Autoscaling for log transformed data. Ensures that all data points are positive.
jnb_AutoScale <- function(x) {
	scaling_factor = 1.1 # factor used to scale the data
	t <- abs(min(x, na.rm=T))
	t <- t * scaling_factor
	r <- x + rep(t, length(x))
	return(r)
}

#######################################################################################
# Produces a summary table for a data.frame or matrix in the workspace.
#######################################################################################
#	Parameters:
#	df: Data frame or matrix containing data values
#	removeNulls: if TRUE, null values will be removed before summary is performed
#######################################################################################
jnb_Summarize <- function(df, removeNulls=TRUE)
{
    colSummary <- data.frame(
          rbind(
            "Minimum"=apply(df, MARGIN=2, FUN=min, na.rm=removeNulls)
            ,"Mean"=apply(df, MARGIN=2, FUN=mean, na.rm=removeNulls)
            ,"STDEV"=apply(df, MARGIN=2, FUN=sd, na.rm=removeNulls)
            ,"Median"=apply(df, MARGIN=2, FUN=median, na.rm=removeNulls)
            ,"Maximum"=apply(df, MARGIN=2, FUN=max, na.rm=removeNulls)
            ,"NULLs"=apply(df, MARGIN=2, FUN=function(x){sum(is.na(x))})
            ,"Present"=apply(df, MARGIN=2, FUN=function(x){sum(!is.na(x))})
            )
        )
	colSummary <- rbind(colSummary
		, "Percent Absent"=(colSummary[6,]*100)/(colSummary[6,] + colSummary[7,])
		, "Percent Present"=(colSummary[7,]*100)/(colSummary[6,] + colSummary[7,]))
		
    rowSummary <- data.frame(
          cbind(
            "Minimum"=apply(df, MARGIN=1, FUN=min, na.rm=removeNulls)
            ,"Mean"=apply(df, MARGIN=1, FUN=mean, na.rm=removeNulls)
            ,"STDEV"=apply(df, MARGIN=1, FUN=sd, na.rm=removeNulls)
            ,"Median"=apply(df, MARGIN=1, FUN=median, na.rm=removeNulls)
            ,"Maximum"=apply(df, MARGIN=1, FUN=max, na.rm=removeNulls)
            ,"NULLs"=apply(df, MARGIN=1, FUN=function(x){sum(is.na(x))})
            ,"Present"=apply(df, MARGIN=1, FUN=function(x){sum(!is.na(x))})
            )
        )
		
    totalSummary <- rbind(
            "Minimum" = min(df, na.rm=removeNulls)
            , "Maximum" = max(df, na.rm=removeNulls)
            , "NULLs"=sum(is.na(df))
            , "Present"=sum(!is.na(df))
            , "Percent Absent" = (sum(is.na(df))*100)/(dim(df)[1]*dim(df)[2])
			, "Percent Present" = (sum(!is.na(df))*100)/(dim(df)[1]*dim(df)[2])
          )
	colnames(totalSummary) <- c("Value")
	return(list("ColumnSummary"=colSummary, "RowSummary"=rowSummary, "TotalSummary"=totalSummary))
}

#######################################################################################
# Exporting csv files with header
#######################################################################################
#	Parameters:
#	df: Data frame or matrix containing data values to export
#	fileName: complete path to export data values to
#	firstColumnHeader: character value used for row names (first column)
#	sepChar: character separating values in export
#	row.names: TRUE if row names are to be exported.
#######################################################################################
jnb_Write <- function(
		df,					# table to write out
		fileName,			# path to output file
		firstColumnHeader,	# header for first column
		sepChar=",",		# character to separate values
		row.names=TRUE)		# indicates whether or not to include the rownames in the first column
{
	out <- file(fileName, "w")
	if (row.names) {
		header <- paste(c(firstColumnHeader, colnames(df)), collapse=sepChar)
		cat(header, "\n", file=out)
		write.table(df, out, sep=sepChar, col.names=FALSE, quote=FALSE, na="")
	} else {
		header <- paste(c(colnames(df)), collapse=sepChar)
		cat(header, "\n", file=out)
		write.table(df, out, sep=sepChar, row.names=row.names, col.names=FALSE, quote=FALSE, na="")
	}
	close(out)
}

#######################################################################################
# Generates an output table for KBase
#######################################################################################
#	Packages Required: reshape and plyr
#	Parameters:
#	x: data frame or matrix
#	factorTable: name of the T_Column_Metadata table
#	factorMergeField: column in factorTable that links to the column names of x
#	factorColumn: column in factorTable to merge in with data
#######################################################################################
jnb_OutputForKBase <- function(
  x,
  factorTable,
  factorMergeField,
  factorColumn) {
  
  require(reshape)
  require(plyr)
  
  mdata <- melt(x)
  colnames(mdata) <- c("MassTagID",
                       "Dataset",
                       "Abundance")
  tmp <- merge(
    x=mdata,
    y=factorTable[,c(factorMergeField,factorColumn)],
    by.x="Dataset",
    by.y=factorMergeField)
  
  tmp <- tmp[!is.na(tmp$Abundance),]
  
  tmpData <- ddply(tmp,
                   c('MassTagID', factorColumn),
                   function(x)
                     c(Abundance_Avg=mean(x$Abundance),
                       Abundance_Median=median(x$Abundance),
                       Abudance_StDev=sd(x$Abundance),
                       ObsCount=length(x$Abundance)))
  
  return(tmpData)
}

#######################################################################################
# Adds Protein Count and Peptide Count to the T_Row_Metadata table
#######################################################################################
#	Packages Required: plyr
#	Parameters:
#	RowMetadataTable: name of T_Row_Metadata table
#	ProteinColumn: column name designating proteins
#	PeptideColumn: column name designating peptides or MassTagIDs
#######################################################################################
jnb_PeptideCountProteinCount <- function(
  RowMetadataTable,
  ProteinColumn,
  PeptideColumn) {
  require(plyr)
  
  tmp <- RowMetadataTable[,
            c(ProteinColumn, PeptideColumn)]
  
  prot <- ddply(tmp,
                ProteinColumn,
                function(x) ProteinCount=nrow(x))
  colnames(prot)[2] <- "PeptideCount"
  
  pep <- ddply(tmp,
               PeptideColumn,
               function(x) PeptideCount=nrow(x))
  colnames(pep)[2] <- "ProteinCount"
  
  RowMetadataTable <- merge(
    x=RowMetadataTable,
    y=pep,
    by.x=PeptideColumn,
    by.y=PeptideColumn)
  
  RowMetadataTable <- merge(
    x=RowMetadataTable,
    y=prot,
    by.x=ProteinColumn,
    by.y=ProteinColumn)
}

#######################################################################################
# Filters RowMetadata and Data tables for minimum and maximum 
# peptide and protein counts
#######################################################################################
#	Parameters:
#	df: original data table
#	RowMetadataTable: name of T_Row_Metadata table
#	ProteinCountColumn: column name (in RowMetadataTable) designating number of proteins the given peptide maps to
#	PeptideCountColumn: column name (in RowMetadataTable) designating number of peptides that map to the given protein
#	PeptideColumnName: column name (in RowMetadataTable) designating peptides
#	MaxProtValue: Maximum number in ProteinCountColumn
#	MinProtValue: Minimum number in ProteinCountColumn
#	MaxPepValue: Maximum number in PeptideCountColumn
#	MinPepValue: Minimum number in PeptideCountColumn
#
#	Returns:
#	List containing:
#		DataTable: filtered data table
#		RowMetaData: filtered row metadata table
#######################################################################################
jnb_FilterByPeptideProteinCount <- function(
  df,
  RowMetadataTable,
  ProteinCountColumn=NULL,
  PeptideCountColumn=NULL,
  PeptideColumnName=NULL,
  MaxProtValue=NULL,
  MinProtValue=NULL,
  MaxPepValue=NULL,
  MinPepValue=NULL) {
  
  tmpRMD <- RowMetadataTable
  
  ########################################
  # Check parameters passed in
  ########################################
  if (is.null(ProteinCountColumn) ||
    is.null(PeptideCountColumn) ||
    is.null(PeptideColumnName)) {
    
    stat <- "All column names must be supplied"
    if (is.null(ProteinCountColumn))
      stop(paste(stat,", missing 'ProteinCountColumn'", sep=""))
    else if (is.null(PeptideCountColumn))
      stop(paste(stat,", missing 'PeptideCountColumn'", sep=""))
    else if (is.null(PeptideColumnName))
      stop(paste(stat,", missing 'PeptideColumnName'", sep=""))
  }
  
  if (is.null(MaxProtValue) && 
    is.null(MinProtValue) &&
    is.null(MaxPepValue) &&
    is.null(MinPepValue))
    stop("Must enter a MaxProtValue, MinProtValue, MaxPepValue, or a MinPepValue")
  
  if (!is.null(MaxProtValue))
    tmpRMD <- RowMetadataTable[
      RowMetadataTable[,ProteinCountColumn]<=MaxProtValue,]
  if (!is.null(MinProtValue))
    tmpRMD <- RowMetadataTable[
      RowMetadataTable[,ProteinCountColumn]>=MinProtValue,]
  if (!is.null(MaxPepValue))
    tmpRMD <- RowMetadataTable[
      RowMetadataTable[,PeptideCountColumn]<=MaxPepValue,]
  if (!is.null(MinPepValue))
    tmpRMD <- RowMetadataTable[
      RowMetadataTable[,PeptideCountColumn]>=MinPepValue,]
  
  ########################################
  # Perform function
  ########################################
  tmpData <- df[which(row.names(df)%in%
    tmpRMD[,PeptideColumnName]),]
  
  theReturn <- list(
    DataTable=tmpData,
    RowMetaData=tmpRMD)
  return(theReturn)
}
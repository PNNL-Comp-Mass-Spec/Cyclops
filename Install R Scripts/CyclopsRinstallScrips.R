##################################################
#  LOADING PACKAGES FOR CYCLOPS
##################################################


install.packages("impute")
install.packages("RODBC")
install.packages("amap")
install.packages("plotrix")
install.packages("reshape")
install.packages("rgl")
install.packages("gplots")
install.packages("outliers")
install.packages("DBI")
install.packages("RSQLite")
install.packages("sqldf")
install.packages("ellipse")
install.packages("pls")
install.packages("Hmisc")
install.packages("rJava")

### Install BetaBinomial from file
### Install MSStats from file

# BioMart
source("http://bioconductor.org/biocLite.R")
biocLite("biomaRt")

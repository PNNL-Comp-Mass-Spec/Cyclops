#------------------------------------------------------------------------------------------------------------
#	Quasi-Likelihood modeling
#	Li et al. (2010) Comparitive shotgun proteomics using spectral count data and quasi-likelihood modeling.
#	JPR. 9(8):4295-305. PMID: 20586475
#
#	Pulled from http://forge.fenchurch.mc.vanderbilt.edu/scm/viewvc.php/branches/IDPicker-3/QuasiTel%20V2.R?root=idpicker&view=log
#	Adapted by Joseph N. Brown, Ph.D.
#------------------------------------------------------------------------------------------------------------

quasitel <- function(data, group1, group2, weight=NULL, rm.SID=TRUE, rm.zero=FALSE, minavgcount=NULL)
{
    # should pass these as arguments instead
    option.contrasts <- getOption('contrasts')
    options(contrasts=c('contr.SAS', 'contr.treatment'))

    # make a grouping factor that will be applicable to the subsetted data
    group <- character()
    if (is.character(group1)) {
        group1 <- which(colnames(data) %in% group1)
    }
    if (is.character(group2)) {
        group2 <- which(colnames(data) %in% group2)
    }
    grp <- union(group1, group2)
    group[group2] <- "group2"
    group[group1] <- "group1"
    group <- factor(group[grp])
    gpl <- split(1:length(group), group)

    # subset the data and groups
    data <- subset(data, select=grp)
    #group1 <- gpl$group1
    #group2 <- gpl$group2

    # filter based on minimum count
    if (!is.null(minavgcount)) {
        grp12count <- apply(data, 1, mean)
        data <- subset(data, grp12count >= minavgcount)
    }

    #grp1zero <- apply(subset(data, select=group1), 1, sum) == 0
    #grp2zero <- apply(subset(data, select=group2), 1, sum) == 0
    #if (rm.zero) {
    #    # only keep features that are not both zero
    #    data <- subset(data, !(grp1zero | grp2zero))
    #} else {
    #    # or add a single count
    #    data[grp1zero, group1[1]] <- 1
    #    data[grp2zero, group2[1]] <- 1
    #}

    # prepare the weight
    if (is.null(weight)) {
        offset <- NULL
        wei <- sapply(gpl, length)
    } else {
        weight <- weight[colnames(data)]
        offset <- log(weight)
        wei <- sapply(gpl, function(x) { sum(weight[x]) })
    }

    Nprotein <- nrow(data)
    result <- matrix(numeric(), nrow=Nprotein, ncol=11)

    for (i in 1:Nprotein) {
        count <- as.numeric(data[i,])

        # poisson p-value
        g1a <- glm(count ~ group, offset=offset, family=poisson)
        g1 <- glm(count ~ 1, offset=offset, family=poisson)
        anovaP <- data.frame(anova(g1, g1a, test="Chisq"))
        Pvalues <- ifelse(anovaP[2,4] < 0.1e-15, 1, anovaP[2,5])

        # quasi p-value
        gquasi1a <- glm(count ~ group, offset=offset, family=quasi(link=log, variance=mu))
        gquasi1 <- glm(count ~ 1, offset=offset, family=quasi(link=log, variance=mu))
        anovaPq <- data.frame(anova(gquasi1, gquasi1a, test="F"))
        Pvaluesq <- ifelse(anovaPq[2,4] < 0.1e-15, 1, anovaPq[2,6])

        lambda <- exp(rev(cumsum(as.numeric(g1a$coef))))
        totcot <- round(wei * lambda, 0)
        rateratio <- log2(lambda[1] / lambda[2])

        sdl <- sapply(gpl, function(x) { sd(count[x]) })
        meanl <- sapply(gpl, function(x) { mean(count[x]) })
        cvl <- mapply("/", sdl, meanl)

        result[i, ] <- c(   totcot,     # 2 items
                            lambda,     # 2 items
                            rateratio,
                            Pvalues,    NA,
                            Pvaluesq,   NA,
                            cvl)        # 2 items
    }
    # fdr adjustment
    for (j in c(6, 8)) {
        result[, j+1] <- p.adjust(result[, j], method="fdr")
    }
    rownames(result) <- rownames(data)
    colnames(result) <- c(  "count1",   "count2",
                            "rates1",   "rates2",
                            "2log(rate1/rate2)",
                            "poisson.p", "poisson.fdr",
                            "quasi.p",  "quasi.fdr",
                            "cv1",      "cv2")
    options(contrasts=option.contrasts)
    result
}
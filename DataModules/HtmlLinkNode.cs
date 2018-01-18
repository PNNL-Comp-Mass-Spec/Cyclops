/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 *
 * Licensed under the Apache License, Version 2.0; you may not use this
 * file except in compliance with the License.  You may obtain a copy of the
 * License at https://opensource.org/licenses/Apache-2.0
 * -----------------------------------------------------*/

namespace Cyclops.DataModules
{
    public class HtmlLinkNode
    {
        #region Properties
        public string Title { get; set; }
        public string Link { get; set; }
        public bool IsInternalLink { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor creating an HtmlLinkNode
        /// </summary>
        public HtmlLinkNode()
        {
        }

        /// <summary>
        /// HtmlLinkNode module that assigns a Cyclops Model
        /// </summary>
        /// <param name="CyclopsModel">Cyclops Model</param>
        public HtmlLinkNode(string Title, string Link, bool IsInternalLink)
        {
            this.Title = Title;
            this.Link = Link;
            this.IsInternalLink = IsInternalLink;
        }
        #endregion
    }
}

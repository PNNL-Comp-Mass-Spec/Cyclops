/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/
 * -----------------------------------------------------
 *
 * Licensed under the 2-Clause BSD License; you may not use this
 * file except in compliance with the License.  You may obtain
 * a copy of the License at https://opensource.org/licenses/BSD-2-Clause
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
        public HtmlLinkNode(string title, string link, bool isInternalLink)
        {
            this.Title = title;
            this.Link = link;
            this.IsInternalLink = isInternalLink;
        }
        #endregion
    }
}

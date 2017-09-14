/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: joseph.brown@pnnl.gov
 * Website: http://omics.pnl.gov/software
 * -----------------------------------------------------
 *
 * Notice: This computer software was prepared by Battelle Memorial Institute,
 * hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the
 * Department of Energy (DOE).  All rights in the computer software are reserved
 * by DOE on behalf of the United States Government and the Contractor as
 * provided in the Contract.
 *
 * NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY WARRANTY, EXPRESS OR
 * IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS SOFTWARE.
 *
 * This notice including this sentence must appear on any copies of this computer
 * software.
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

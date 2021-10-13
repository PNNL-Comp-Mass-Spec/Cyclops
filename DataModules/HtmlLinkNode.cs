/* Written by Joseph N. Brown
 * for the Department of Energy (PNNL, Richland, WA)
 * Battelle Memorial Institute
 * E-mail: proteomics@pnnl.gov
 * Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
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
        public string Title { get; }
        public string Link { get; }
        public bool IsInternalLink { get; }

        /// <summary>
        /// HtmlLinkNode module that assigns a Cyclops Model
        /// </summary>
        public HtmlLinkNode(string title, string link, bool isInternalLink)
        {
            Title = title;
            Link = link;
            IsInternalLink = isInternalLink;
        }
    }
}

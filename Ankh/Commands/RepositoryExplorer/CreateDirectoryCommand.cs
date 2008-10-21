// $Id$
using System;
using System.Windows.Forms;
using Ankh.Ids;
using SharpSvn;
using Ankh.Selection;
using Ankh.Scc;
using Ankh.UI.SccManagement;

namespace Ankh.Commands.RepositoryExplorer
{
    /// <summary>
    /// Command to creates a new directory here in the Repository Explorer.
    /// </summary>
    [Command(AnkhCommand.NewDirectory)]
    class CreateDirectoryCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            bool enabled = false;
            
            ISvnRepositoryItem item = GetValidSelectedItem(e.Selection);
            if (item != null
                && item.NodeKind == SvnNodeKind.Directory)
            {
                enabled = true;
            }
            e.Enabled = enabled;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            ISvnRepositoryItem selected = GetValidSelectedItem(e.Selection);
            if (selected == null) { return; }

            string directoryName = string.Empty;

            using (CreateDirectoryDialog dlg = new CreateDirectoryDialog())
            {
                DialogResult result = dlg.ShowDialog(e.Context);

                directoryName = dlg.NewDirectoryName;

                if (result != DialogResult.OK || string.IsNullOrEmpty(directoryName))
                    return;

                string log = dlg.LogMessage;

                // Handle special characters like on local path
                Uri uri = SvnTools.AppendPathSuffix(selected.Uri, directoryName);

                ProgressRunnerResult prResult =
                    e.GetService<IProgressRunner>().Run(
                    CommandStrings.CreatingDirectories,
                    delegate(object sender, ProgressWorkerArgs ee)
                    {
                        SvnCreateDirectoryArgs args = new SvnCreateDirectoryArgs();
                        args.ThrowOnError = false;
                        args.CreateParents = true;
                        args.LogMessage = log;
                        ee.Client.RemoteCreateDirectory(uri, args);
                    }
                    );

                if (prResult.Succeeded)
                {
                    selected.RefreshItem(false);
                }
            }
        }

        /// <summary>
        /// Get the selected ISvnRepositoryItem
        /// </summary>
        /// <param name="context">ISelectionContext</param>
        /// <returns>ISvnRepositoryItem or null</returns>
        private Ankh.Scc.ISvnRepositoryItem GetValidSelectedItem(ISelectionContext context)
        {
            Ankh.Scc.ISvnRepositoryItem result = null;
            int counter = 0;
            foreach (Ankh.Scc.ISvnRepositoryItem i in context.GetSelection<Ankh.Scc.ISvnRepositoryItem>())
            {
                counter++;
                if (counter > 1) { return null; } // multiple selection
                if (i.Origin.RepositoryRoot != null)
                {
                    result = i;
                }
            }
            return result;
        }
    }
}




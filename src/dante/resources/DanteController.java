/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package dante.resources;

import dante.DanteView;
import java.beans.PropertyChangeEvent;
import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;
import javax.swing.JFrame;
import javax.swing.JOptionPane;
import javax.swing.table.DefaultTableModel;
import javax.swing.tree.DefaultTreeModel;

/**
 *
 * @author  Joseph N. Brown
 * @date    6/3/2011
 */
public class DanteController extends DanteAbstractController {
    
    protected DanteModel dm;
    protected DanteView dv;
    
    public DanteController(DanteView viewer) {
        dm = new DanteModel();
        dv = viewer;
    }

    public void propertyChange(PropertyChangeEvent evt) {
        throw new UnsupportedOperationException("Not supported yet.");
    }
    
    public void openFile(String FileName) {
//        System.out.println("Made it into the Controller!");
        
        // Determine if it's coming from a text file
        if (FileName.endsWith(".txt") | FileName.endsWith(".csv")) {
            DataImportDialog dig = new DataImportDialog(this, FileName);
            dig.setVisible(true);
        } 
        // Determine if it's coming from a SQLite database
        else if (FileName.endsWith(".db3") | FileName.endsWith(".db"))
        {
            // SQLite database table
        }
        else 
        {
            // Unrecognized file format
            
            JOptionPane.showMessageDialog(new JFrame(), 
                    "File must be .txt (tab-delimited), " +
                    ".csv (comma separated value), or " + 
                    ".db3 (SQLite database file)", 
                    "Unrecognized File Format", 
                    JOptionPane.ERROR_MESSAGE);
        }
        
    }
    
    public void loadProject(String ProjectName) {
        // Tell DanteModel to load a project
        dm.loadWorkspace(ProjectName);
        dm.GetUpDate();
        
        dv.RefreshTableDatasets();
    }
    
    public void setWorkspaceName(String FileName) {
        dm.set_WorkspaceFileName(FileName);
    }
    
    public String getWorkspaceName() {
        return dm.get_WorkspaceFileName();
    }
    
    public void testIrisData() {
        dm.testIrisDataset();
    }
    
    public DefaultTableModel RefreshTableDatasets() {
        return dm.RefreshTableDatasets();
    }
    
    public void SaveWorkspace(String FileName) {
        dm.SaveWorkspace(FileName);
    }
    
    public void CloseWorkspace() {
        dm.CloseWorkspace();
    }
    
    public void ImportDataTableFromFile(DanteImportTableHandler handler){
        dm.ImportTableFromFile(handler);
        
        dv.RefreshTreeDatasets();
        dv.RefreshTableDatasets();
    }
    
    public DefaultTreeModel RefreshTreeDatasets() {
        return dm.RefreshTreeDatasets();
    }
    
    public DefaultTableModel GetDataset(String DatasetName) {
        return dm.GetDataset(DatasetName);
    }
    
    public ArrayList<String> GetObjectsInWorkspace() {
        return dm.GetObjectsInWorkspace();
    }
}

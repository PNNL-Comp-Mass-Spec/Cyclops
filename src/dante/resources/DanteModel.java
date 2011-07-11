/*   
 * 
 */
package dante.resources;

import java.util.ArrayList;
import java.util.Enumeration;
import javax.swing.JFrame;
import javax.swing.JOptionPane;
import javax.swing.table.DefaultTableModel;
import javax.swing.tree.DefaultMutableTreeNode;
import javax.swing.tree.DefaultTreeModel;
import org.rosuda.JRI.REXP;

import org.rosuda.JRI.RMainLoopCallbacks;
import org.rosuda.JRI.RVector;
import org.rosuda.JRI.Rengine;

/**
 *
 * @author  Joseph N. Brown
 * @date    6/3/2011
 */
public class DanteModel extends DanteAbstractModel {
    
    /// PROPERTIES
    // new R-engine
    Rengine re;
    protected String s_WorkspaceFileName = "NewProject";

    /**
     * Get the value of s_WorkspaceFileName
     *
     * @return the value of s_WorkspaceFileName
     */
    public String get_WorkspaceFileName() {
        return s_WorkspaceFileName;
    }

    /**
     * Set the value of s_WorkspaceFileName
     *
     * @param s_WorkspaceFileName new value of s_WorkspaceFileName
     */
    public void set_WorkspaceFileName(String s_WorkspaceFileName) {
        this.s_WorkspaceFileName = s_WorkspaceFileName;
    }

    
    
    public DanteModel() {
        re = new Rengine (new String [] {"--vanilla"}, false, null);
        if (!re.waitForR())
        {
            JOptionPane.showMessageDialog(new JFrame(), "Cannot load R");
            return;
        }
    }
    
    /**
     * Provides the means to set or reset the model to
     * a default state
     */
    public void initDefault() {
        /// SET MODEL BACK TO DEFAULT
    }
    
    /// Import File from text or from SQLite database
    public void ImportTableFromFile(DanteImportTableHandler handler) {
        System.out.println("Saving new table: " + handler.getTableName());
        
        // Determine what type of table to create (Data table, 
        // Column Metadata, Row Metadata
        if (handler.isDataTableImport()) {
          String s_Statement = "";
          
          // determine what type of file to import
          if (handler.getFileName().endsWith(".txt")) { // tab-delimited text file
              try {
                  re.eval(handler.getTableName() +
                      " <- read.table(file=\"" +
                      handler.getFileName() +
                      "\", sep=\"\t\", header=T, quote=\"\")"
                      );
              } catch(Exception e) {
                  JOptionPane.showMessageDialog(new JFrame(), e.toString());
              }
          }
          else if (handler.getFileName().endsWith(".csv")) { // CSV file
              try {
                  s_Statement = handler.getTableName() +
                      " <- data.frame(read.csv(file=\"" +
                      handler.getFileName() +
                      "\"))";
                  System.out.println(s_Statement);
                  re.eval(s_Statement);
              } catch(Exception e) {
                  JOptionPane.showMessageDialog(new JFrame(), e.toString());
              }
          }      
                
          if (handler.isIncludeRowMetaData()) {
            try {
                System.out.println("Creating Row Metadata...");
                System.out.println("TableName: " + handler.getRowMetadataTableName());
                s_Statement = handler.getRowMetadataTableName() + " <- " +
                "unique(cbind(";
                for (int i = 0; i < handler.getRowMetadataColumns().length; i++) {
                    if (i < handler.getRowMetadataColumns().length - 1) {
                        s_Statement += "\"" + handler.getRowMetadataColumns()[i] +
                                "\"=as.character(" + handler.getTableName() + "$" +
                                handler.getRowMetadataColumns()[i] + "), ";
                    }
                    else {
                        s_Statement += "\"" + handler.getRowMetadataColumns()[i] +
                                "\"=as.character(" + handler.getTableName() + "$" +
                                handler.getRowMetadataColumns()[i] + ")))";
                    }
                }
                System.out.println(s_Statement);
                re.eval(s_Statement);
                } catch(Exception e) {
                  JOptionPane.showMessageDialog(new JFrame(), e.toString());
              }
          }

          try {
                s_Statement = handler.getTableName() + " <- unique(cbind(\"" +
                  handler.getUniqueRowID() + "\"=as.character(" +
                  handler.getTableName() + "$" + handler.getUniqueRowID() + "), ";
                for (int i = 0; i < handler.getColumnsToKeep().length; i++) {
                  if (i < handler.getColumnsToKeep().length - 1) {
                      s_Statement += "\"" + handler.getColumnsToKeep()[i] + "\"=" +
                              handler.getTableName() + "$" + 
                              handler.getColumnsToKeep()[i] + ", ";
                  }
                  else {
                      s_Statement += "\"" + handler.getColumnsToKeep()[i] + "\"=" +
                              handler.getTableName() + "$" + 
                              handler.getColumnsToKeep()[i] + "))";
                  }
                }
                System.out.println(s_Statement);
                re.eval(s_Statement);

                s_Statement = "rownames(" + handler.getTableName() + ") <- " + 
                      handler.getTableName() + "[,1]";
                System.out.println(s_Statement);
                re.eval(s_Statement);

                // Now remove that first column
                s_Statement = handler.getTableName() + " <- " +
                      handler.getTableName() + "[,-1]";
                System.out.println(s_Statement);
                re.eval(s_Statement);
              } catch(Exception e) {
                  JOptionPane.showMessageDialog(new JFrame(), e.toString());
              }
          } else if (handler.getFileName().endsWith(".db3") |
                  handler.getFileName().endsWith(".db")) { // SQLite table
          
        } else if (handler.isColumnMetadataImport()) {
            
        } else if (handler.isRowMetadataImport()) {
            
        }
    }
    
    public void loadWorkspace(String WorkspaceName) {
        System.out.println("Loading: " + WorkspaceName);
        
        try
        {
            String s_Load = "load(\"" + WorkspaceName + "\")";
            System.out.println(s_Load);
            re.eval(s_Load);
            this.set_WorkspaceFileName(WorkspaceName);
                        
        } catch (Exception e) {
            JOptionPane.showMessageDialog(new JFrame(), "Error loading Project File:\n" +
                    WorkspaceName + "\n" + e.toString());
        }
        System.out.println("Workspace loaded...");
    }
    
    public void testIrisDataset() {
        try 
        {
            System.out.println("Grabbing the iris data...");
            REXP x;
            re.eval("data(iris)", false);
            System.out.println(x=re.eval("iris"));
        
            // generic vectors are RVector to accomodate names
            RVector v = x.asVector();
            if (v.getNames()!=null) {
                    System.out.println("has names:");
                    for (Enumeration e = v.getNames().elements() ; e.hasMoreElements() ;) {
                        	System.out.println(e.nextElement());
                    }
            } 
            
            System.out.println("\nEvaluating ls()...");
            x = re.eval("ls()");
            String[] sa = x.asStringArray();
            System.out.println("Dataset contains: " + sa.length);
            for (int i = 0; i < sa.length; i++) {
                System.out.println(sa[i]);
            }
            
        } catch (Exception e) {
            System.out.println("EX:"+e);
            e.printStackTrace();
	}
    }
    
    public DefaultTableModel GetDataset(String DatasetName) {
        DefaultTableModel dtm = new DefaultTableModel();
        String s_Class = re.eval("class(" + DatasetName + ")").asString();
//        System.out.println(DatasetName + " Class = " + s_Class);
        
        
        
        if (s_Class.equals("data.frame") | s_Class.equals("matrix"))
        {
            int i_ColNum = re.eval("ncol(" + DatasetName + ")").asInt();
//            System.out.println(DatasetName + " has " + i_ColNum + " columns");
            int i_RowNum = re.eval("nrow(" + DatasetName + ")").asInt();
//            System.out.println(DatasetName + " has " + i_RowNum + " rows");
        
            // Grab the column and rowname information
            String[] s_Headers = new String[i_ColNum];
            String[] s_RowNames = new String[i_RowNum];
            REXP x = re.eval("colnames(" + DatasetName + ")");
            s_Headers = x.asStringArray();
            x = re.eval("rownames(" + DatasetName + ")");
            s_RowNames = x.asStringArray();
            
            // Get the data values in the dataset
            x = re.eval(DatasetName);
            String[] s_Values = new String[i_ColNum * i_RowNum];
            s_Values = x.asStringArray();
            double[][] d_Values = new double[i_RowNum][i_ColNum];
            if (s_Values == null) {
                d_Values = x.asDoubleMatrix();
            }
            
            // Add the table headers
            dtm.addColumn("row.names");
            System.out.println("Dataset Headers:");
            for (int i = 0; i < i_ColNum; i++) {
                if (s_Headers != null) {
                    dtm.addColumn(s_Headers[i]);
//                    System.out.println(s_Headers[i]);
                } else {
                    int t = i + 1;
                    String s = "Field" + Integer.toString(t);
                    dtm.addColumn(s);
//                    System.out.println(s);
                }
            }
            
            // Now assemble the table for export
            int i_ValueIterator = 0;
            for (int r = 0; r < i_RowNum; r++) {
                String[] s_Row = new String[i_ColNum + 1];
                
//                System.out.println("Adding row names...");
                if (s_RowNames != null) {
                    s_Row[0] = s_RowNames[r]; // add the row.names value
                } else {
                    s_Row[0] = Integer.toString(r+1);
                }
                
//                System.out.println("Adding values...");
                
                for (int c = 0; c < i_ColNum; c++) {
                    if (s_Values != null) {
                        s_Row[c+1] = s_Values[i_ValueIterator];
                        i_ValueIterator++;
                    } else {
                        s_Row[c+1] = Double.toString(d_Values[r][c]);
                    }
                }
                dtm.addRow(s_Row);
            }
        }
        return dtm;
    }
    
    public ArrayList<String> GetObjectsInWorkspace() {
        REXP x = re.eval("ls()");
        String[] sa = x.asStringArray();
        ArrayList<String> l_Obj = new ArrayList<String>();
        for (int i = 0; i < sa.length; i++) {
            l_Obj.add(sa[i]);
        }
        return l_Obj;
    }
    
    public DefaultTableModel RefreshTableDatasets() {
        DefaultTableModel dtm = new DefaultTableModel();
        // Column Headers
        String[] s_ColHeaders = new String[] {
            "Name", "Class", "Rows", "Columns"
        };
        
        // Add column headers to model
        for (int i = 0; i < s_ColHeaders.length; i++) {
            dtm.addColumn(s_ColHeaders[i]);
        }
        REXP x = re.eval("ls()");
        String[] sa = x.asStringArray();
        
        for (int i = 0; i < sa.length; i++) {
            x = re.eval("class(" + sa[i] + ")");
            String s_Class = x.asString();
            int i_Row = 0, i_Col = 0;
            
            if (s_Class.equals("data.frame") | 
                s_Class.equals("matrix")) {
                x = re.eval("nrow(" + sa[i] + ")");
                i_Row = x.asInt();
                x = re.eval("ncol(" + sa[i] + ")");
                i_Col = x.asInt();
            }
            else {
                x = re.eval("length(" + sa[i] + ")");
                i_Row = x.asInt();
            }
            
            String[] s_Row = new String[] {
              sa[i], s_Class, 
              Integer.toString(i_Row), 
              Integer.toString(i_Col)
            };
            dtm.addRow(s_Row);
        }
        
        return dtm;
    }
    
    /// This method creates a DefaultTreeModel from the RData datasets
    /// to update the view
    public DefaultTreeModel RefreshTreeDatasets() {
        // Grabs the s_WorkspaceFileName (users RData workspace), and uses
        // the file name as the root node
        String s_FileName = "NewProject";
        if (!s_WorkspaceFileName.equals("NewProject")) {
            Filename f_WorkspaceName = new Filename(s_WorkspaceFileName, '/', '.');
            s_FileName = f_WorkspaceName.filename();
        }
        DefaultMutableTreeNode rootNode = new DefaultMutableTreeNode(s_FileName);
        DefaultTreeModel treeModel = new DefaultTreeModel(rootNode);
        
        REXP x = re.eval("ls()");
        String[] sa = x.asStringArray();
        
        for (int i = 0; i < sa.length; i++) {
            DefaultMutableTreeNode newNode = new DefaultMutableTreeNode(sa[i]);
            rootNode.add(newNode);
            
            REXP rows = re.eval("nrow(" + sa[i] + ")");
            int i_Rows = rows.asInt();
            
            REXP cols = re.eval("ncol(" + sa[i] + ")");
            int i_Cols = cols.asInt();
            DefaultMutableTreeNode newNodeRowsAndCols = new 
                    DefaultMutableTreeNode(i_Rows+" rows; " + i_Cols + " columns");
            newNode.add(newNodeRowsAndCols);
            
        }
        
        return treeModel;
    }
    
    public void GetUpDate() {
        
        
        
        REXP obj = re.eval("ls()", true);
        
        
//        String[] s = obj.asStringArray();
//        System.out.println("Testing ls()");
//        if (obj == null) {
//            System.out.println("Looks like the workspace is empty!");
//            
//        }
//        else {
//            System.out.println("obj = " + obj.toString());
//            System.out.println("Class = " + obj.getClass());
//            System.out.println("Content = " + obj.getContent());
//            System.out.println("Length of the array: " + s.length);
//            for (int i = 0; i < s.length; i++) {
//                System.out.println(s[i]);
//            }
            
//            RVector v = obj.asVector();
//            System.out.println("Analyzing the vector, length = " + v.size());
//            for (Enumeration e = v.getNames().elements(); e.hasMoreElements();) {
//                System.out.println(e.nextElement());
//            }
//        }
        
    }
    
    public void SaveWorkspace(String FileName) {
        String s_Save = "save.image(\"" + FileName + "\")";
        System.out.println("Saving Workspace...");
        System.out.println(s_Save);
        re.eval(s_Save);
        JOptionPane.showMessageDialog(new JFrame(), 
                "Workspace was successfully saved.\n" + s_Save,
                "Saved Workspace", JOptionPane.INFORMATION_MESSAGE);
    }
    
    public void CloseWorkspace() {
        re.eval("rm(list=ls())");
    }
}

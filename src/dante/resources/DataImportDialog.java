/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

/*
 * DataImportDialog.java
 *
 * Created on Jul 4, 2011, 2:25:55 PM
 */
package dante.resources;

import java.awt.Color;
import java.awt.Component;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.util.StringTokenizer;
import javax.swing.ComboBoxModel;
import javax.swing.DefaultListModel;
import javax.swing.JOptionPane;
import javax.swing.ListModel;
import javax.swing.table.DefaultTableModel;
import javax.swing.table.TableCellRenderer;

/**
 *
 * @author d3x050
 */
public class DataImportDialog extends javax.swing.JFrame {

    String s_FileName = "";
    DanteController controller;
    DefaultListModel listModel = new DefaultListModel();
    DefaultListModel listToIncludeModel = new DefaultListModel();
    DefaultListModel listRowModel = new DefaultListModel();
    DefaultTableModel dtm_Table = new DefaultTableModel();

    /** Creates new form DataImportDialog */
    public DataImportDialog() {
        initComponents();
        
        lb_Fields.setModel(listModel);
        lb_FieldsToInclude.setModel(listToIncludeModel);
        lb_RowMetadataFields.setModel(listRowModel);
    }
    
    public DataImportDialog(DanteController dc, String FileName) {
        initComponents();
        controller = dc;
        s_FileName = FileName;
        
        try {
            BufferedReader br = new BufferedReader(new FileReader(s_FileName));
            String s_Line = "";
            StringTokenizer st = null;  
            int lineNumber = 0, tokenNumber = 0, i_ColNum = 0;;
            
            // Set the field separator character
            File f = new File(s_FileName);
            String s_Sep = "\t";
            if (f.getAbsolutePath().endsWith(".csv"))
            {
                s_Sep = ",";
            }
            
            // read in the file
            while((s_Line = br.readLine()) != null)
            {
                lineNumber++;
                                
                // break line using separator
                st = new StringTokenizer(s_Line, s_Sep);
                
                // Add the fields from the first line to the listbox
                if (lineNumber==1) 
                {
                    i_ColNum = st.countTokens();
                    
//                    listModel = new DefaultListModel();
                    while(st.hasMoreTokens())
                    {
                        String s_Field = st.nextToken();
                        listModel.addElement(s_Field);
                        this.cmb_UniqueRowID.addItem(s_Field);
                        this.dtm_Table.addColumn(s_Field);
                    }
                    lb_Fields.setModel(listModel);
                    lb_FieldsToInclude.setModel(listToIncludeModel);
                    lb_RowMetadataFields.setModel(listRowModel);
                }
                else {
                    if (i_ColNum != st.countTokens()) {
                        JOptionPane.showMessageDialog(this, "Error on line: " + 
                                lineNumber + ".\nThe number of columns in this " +
                                "row does not match the number of columns."+
                                "Columns contain " + i_ColNum + " elements.\n" +
                                "This line contains " + st.countTokens() + " elements.",
                                "Error reading table.\n",                                
                                JOptionPane.ERROR_MESSAGE);
                        this.setVisible(false);
                        break;
                    }
                    else {
                        String[] s_Row = new String[st.countTokens()];
                        for (int i = 0; i < s_Row.length; i++) {
                            s_Row[i] = st.nextToken();
                        }
                        dtm_Table.addRow(s_Row);
                    }
                }
            }
            
            this.tbl_File.setModel(dtm_Table);
        }
        catch(IOException ioe) {
            JOptionPane.showMessageDialog(this, ioe.toString(), 
                        "Error Reading File", JOptionPane.ERROR_MESSAGE);
        }
    }
    
    protected DanteImportTableHandler FillTableHandler() {
        DanteImportTableHandler dith = new DanteImportTableHandler();
        dith.setFileName(s_FileName);
        dith.setTableName(this.tb_TableName.getText());
        dith.setRowMetadataTableName(this.tb_RowMetadataTableName.getText());
        dith.setUniqueRowID(this.cmb_UniqueRowID.getSelectedItem().toString());
        dith.setIncludeRowMetaData(this.cb_CreateRowmetadata.isSelected());
        ListModel lm = this.lb_FieldsToInclude.getModel();
        String[] columns2include = new String[lm.getSize()];
        for (int i=0; i < columns2include.length; i++) {
            columns2include[i] = lm.getElementAt(i).toString();
            }
        dith.setColumnsToKeep(columns2include);
                
        ListModel rm = this.lb_RowMetadataFields.getModel();
        String[] rowdata = new String[rm.getSize()];
        for (int i = 0; i < rm.getSize(); i++) {
            rowdata[i] = rm.getElementAt(i).toString();
        }
        dith.setRowMetadataColumns(rowdata);
        
        return dith;
    }

    /** This method is called from within the constructor to
     * initialize the form.
     * WARNING: Do NOT modify this code. The content of this method is
     * always regenerated by the Form Editor.
     */
    @SuppressWarnings("unchecked")
    // <editor-fold defaultstate="collapsed" desc="Generated Code">//GEN-BEGIN:initComponents
    private void initComponents() {

        buttonGroup1 = new javax.swing.ButtonGroup();
        jPanel1 = new javax.swing.JPanel();
        jLabel2 = new javax.swing.JLabel();
        jPanel2 = new javax.swing.JPanel();
        rb_DataFile = new javax.swing.JRadioButton();
        rb_ColMetadata = new javax.swing.JRadioButton();
        rb_RowMetadata = new javax.swing.JRadioButton();
        jPanel3 = new javax.swing.JPanel();
        tb_TableName = new javax.swing.JTextField();
        jPanel4 = new javax.swing.JPanel();
        jScrollPane1 = new javax.swing.JScrollPane();
        lb_Fields = new javax.swing.JList();
        jPanel5 = new javax.swing.JPanel();
        cmb_UniqueRowID = new javax.swing.JComboBox();
        jPanel6 = new javax.swing.JPanel();
        jScrollPane2 = new javax.swing.JScrollPane();
        lb_FieldsToInclude = new javax.swing.JList();
        btn_IncludeDataColumn = new javax.swing.JButton();
        btn_RemoveDataColumn = new javax.swing.JButton();
        cb_CreateRowmetadata = new javax.swing.JCheckBox();
        jPanel7 = new javax.swing.JPanel();
        jScrollPane3 = new javax.swing.JScrollPane();
        lb_RowMetadataFields = new javax.swing.JList();
        btn_IncludeRowColumn = new javax.swing.JButton();
        btn_RemoveRowColumn = new javax.swing.JButton();
        jPanel10 = new javax.swing.JPanel();
        tb_RowMetadataTableName = new javax.swing.JTextField();
        jPanel8 = new javax.swing.JPanel();
        jScrollPane4 = new javax.swing.JScrollPane();
        tbl_File = new javax.swing.JTable(){
            public Component prepareRenderer
            (TableCellRenderer renderer,int Index_row, int Index_col) {
                Component comp = super.prepareRenderer(renderer, Index_row, Index_col);
                //even index, selected or not selected
                if (Index_row % 2 == 0 && !isCellSelected(Index_row, Index_col)) {
                    comp.setBackground(Color.white);
                }
                else {
                    comp.setBackground(Color.lightGray);
                }
                return comp;
            }
        };
        jPanel9 = new javax.swing.JPanel();
        btn_OK = new javax.swing.JButton();
        btn_Cancel = new javax.swing.JButton();

        setName("Form"); // NOI18N
        setResizable(false);

        jPanel1.setName("jPanel1"); // NOI18N

        org.jdesktop.application.ResourceMap resourceMap = org.jdesktop.application.Application.getInstance(dante.DanteApp.class).getContext().getResourceMap(DataImportDialog.class);
        jLabel2.setFont(resourceMap.getFont("jLabel2.font")); // NOI18N
        jLabel2.setText(resourceMap.getString("jLabel2.text")); // NOI18N
        jLabel2.setName("jLabel2"); // NOI18N

        jPanel2.setBorder(javax.swing.BorderFactory.createTitledBorder(resourceMap.getString("jPanel2.border.title"))); // NOI18N
        jPanel2.setName("jPanel2"); // NOI18N

        buttonGroup1.add(rb_DataFile);
        rb_DataFile.setSelected(true);
        rb_DataFile.setText(resourceMap.getString("rb_DataFile.text")); // NOI18N
        rb_DataFile.setName("rb_DataFile"); // NOI18N
        rb_DataFile.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                rb_DataFileActionPerformed(evt);
            }
        });

        buttonGroup1.add(rb_ColMetadata);
        rb_ColMetadata.setText(resourceMap.getString("rb_ColMetadata.text")); // NOI18N
        rb_ColMetadata.setName("rb_ColMetadata"); // NOI18N
        rb_ColMetadata.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                rb_ColMetadataActionPerformed(evt);
            }
        });

        buttonGroup1.add(rb_RowMetadata);
        rb_RowMetadata.setText(resourceMap.getString("rb_RowMetadata.text")); // NOI18N
        rb_RowMetadata.setName("rb_RowMetadata"); // NOI18N
        rb_RowMetadata.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                rb_RowMetadataActionPerformed(evt);
            }
        });

        org.jdesktop.layout.GroupLayout jPanel2Layout = new org.jdesktop.layout.GroupLayout(jPanel2);
        jPanel2.setLayout(jPanel2Layout);
        jPanel2Layout.setHorizontalGroup(
            jPanel2Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel2Layout.createSequentialGroup()
                .add(jPanel2Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
                    .add(rb_DataFile)
                    .add(rb_ColMetadata)
                    .add(rb_RowMetadata))
                .addContainerGap(org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );
        jPanel2Layout.setVerticalGroup(
            jPanel2Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel2Layout.createSequentialGroup()
                .add(rb_DataFile)
                .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED)
                .add(rb_ColMetadata)
                .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED)
                .add(rb_RowMetadata)
                .addContainerGap(org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );

        jPanel3.setBorder(javax.swing.BorderFactory.createTitledBorder(resourceMap.getString("jPanel3.border.title"))); // NOI18N
        jPanel3.setName("jPanel3"); // NOI18N

        tb_TableName.setText(resourceMap.getString("tb_TableName.text")); // NOI18N
        tb_TableName.setName("tb_TableName"); // NOI18N

        org.jdesktop.layout.GroupLayout jPanel3Layout = new org.jdesktop.layout.GroupLayout(jPanel3);
        jPanel3.setLayout(jPanel3Layout);
        jPanel3Layout.setHorizontalGroup(
            jPanel3Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(tb_TableName, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, 179, Short.MAX_VALUE)
        );
        jPanel3Layout.setVerticalGroup(
            jPanel3Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(tb_TableName, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
        );

        jPanel4.setBorder(javax.swing.BorderFactory.createTitledBorder(resourceMap.getString("jPanel4.border.title"))); // NOI18N
        jPanel4.setName("jPanel4"); // NOI18N

        jScrollPane1.setName("jScrollPane1"); // NOI18N

        lb_Fields.setName("lb_Fields"); // NOI18N
        jScrollPane1.setViewportView(lb_Fields);

        org.jdesktop.layout.GroupLayout jPanel4Layout = new org.jdesktop.layout.GroupLayout(jPanel4);
        jPanel4.setLayout(jPanel4Layout);
        jPanel4Layout.setHorizontalGroup(
            jPanel4Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jScrollPane1, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, 179, Short.MAX_VALUE)
        );
        jPanel4Layout.setVerticalGroup(
            jPanel4Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jScrollPane1, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, 317, Short.MAX_VALUE)
        );

        jPanel5.setBorder(javax.swing.BorderFactory.createTitledBorder(resourceMap.getString("jPanel5.border.title"))); // NOI18N
        jPanel5.setName("jPanel5"); // NOI18N

        cmb_UniqueRowID.setToolTipText(resourceMap.getString("cmb_UniqueRowID.toolTipText")); // NOI18N
        cmb_UniqueRowID.setName("cmb_UniqueRowID"); // NOI18N

        org.jdesktop.layout.GroupLayout jPanel5Layout = new org.jdesktop.layout.GroupLayout(jPanel5);
        jPanel5.setLayout(jPanel5Layout);
        jPanel5Layout.setHorizontalGroup(
            jPanel5Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(cmb_UniqueRowID, 0, 0, Short.MAX_VALUE)
        );
        jPanel5Layout.setVerticalGroup(
            jPanel5Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(cmb_UniqueRowID, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
        );

        jPanel6.setBorder(javax.swing.BorderFactory.createTitledBorder(resourceMap.getString("jPanel6.border.title"))); // NOI18N
        jPanel6.setName("jPanel6"); // NOI18N

        jScrollPane2.setName("jScrollPane2"); // NOI18N

        lb_FieldsToInclude.setName("lb_FieldsToInclude"); // NOI18N
        jScrollPane2.setViewportView(lb_FieldsToInclude);

        btn_IncludeDataColumn.setText(resourceMap.getString("btn_IncludeDataColumn.text")); // NOI18N
        btn_IncludeDataColumn.setActionCommand(resourceMap.getString("btn_IncludeDataColumn.actionCommand")); // NOI18N
        btn_IncludeDataColumn.setName("btn_IncludeDataColumn"); // NOI18N
        btn_IncludeDataColumn.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                btn_IncludeDataColumnActionPerformed(evt);
            }
        });

        btn_RemoveDataColumn.setText(resourceMap.getString("btn_RemoveDataColumn.text")); // NOI18N
        btn_RemoveDataColumn.setActionCommand(resourceMap.getString("btn_RemoveDataColumn.actionCommand")); // NOI18N
        btn_RemoveDataColumn.setName("btn_RemoveDataColumn"); // NOI18N
        btn_RemoveDataColumn.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                btn_RemoveDataColumnActionPerformed(evt);
            }
        });

        org.jdesktop.layout.GroupLayout jPanel6Layout = new org.jdesktop.layout.GroupLayout(jPanel6);
        jPanel6.setLayout(jPanel6Layout);
        jPanel6Layout.setHorizontalGroup(
            jPanel6Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel6Layout.createSequentialGroup()
                .add(jPanel6Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
                    .add(btn_IncludeDataColumn, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, 51, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                    .add(btn_RemoveDataColumn, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, 51, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE))
                .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED, 16, Short.MAX_VALUE)
                .add(jScrollPane2, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, 181, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE))
        );
        jPanel6Layout.setVerticalGroup(
            jPanel6Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel6Layout.createSequentialGroup()
                .add(26, 26, 26)
                .add(btn_IncludeDataColumn)
                .addPreferredGap(org.jdesktop.layout.LayoutStyle.UNRELATED)
                .add(btn_RemoveDataColumn)
                .addContainerGap(233, Short.MAX_VALUE))
            .add(jScrollPane2, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, 317, Short.MAX_VALUE)
        );

        cb_CreateRowmetadata.setText(resourceMap.getString("cb_CreateRowmetadata.text")); // NOI18N
        cb_CreateRowmetadata.setName("cb_CreateRowmetadata"); // NOI18N
        cb_CreateRowmetadata.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                cb_CreateRowmetadataActionPerformed(evt);
            }
        });

        jPanel7.setBorder(javax.swing.BorderFactory.createTitledBorder(resourceMap.getString("jPanel7.border.title"))); // NOI18N
        jPanel7.setEnabled(false);
        jPanel7.setName("jPanel7"); // NOI18N

        jScrollPane3.setName("jScrollPane3"); // NOI18N

        lb_RowMetadataFields.setEnabled(false);
        lb_RowMetadataFields.setName("lb_RowMetadataFields"); // NOI18N
        jScrollPane3.setViewportView(lb_RowMetadataFields);

        btn_IncludeRowColumn.setText(resourceMap.getString("btn_IncludeRowColumn.text")); // NOI18N
        btn_IncludeRowColumn.setActionCommand(resourceMap.getString("btn_IncludeRowColumn.actionCommand")); // NOI18N
        btn_IncludeRowColumn.setEnabled(false);
        btn_IncludeRowColumn.setName("btn_IncludeRowColumn"); // NOI18N
        btn_IncludeRowColumn.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                btn_IncludeRowColumnActionPerformed(evt);
            }
        });

        btn_RemoveRowColumn.setText(resourceMap.getString("btn_RemoveRowColumn.text")); // NOI18N
        btn_RemoveRowColumn.setEnabled(false);
        btn_RemoveRowColumn.setName("btn_RemoveRowColumn"); // NOI18N
        btn_RemoveRowColumn.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                btn_RemoveRowColumnActionPerformed(evt);
            }
        });

        jPanel10.setBorder(javax.swing.BorderFactory.createTitledBorder(resourceMap.getString("jPanel10.border.title"))); // NOI18N
        jPanel10.setName("jPanel10"); // NOI18N

        tb_RowMetadataTableName.setText(resourceMap.getString("tb_RowMetadataTableName.text")); // NOI18N
        tb_RowMetadataTableName.setEnabled(false);
        tb_RowMetadataTableName.setName("tb_RowMetadataTableName"); // NOI18N

        org.jdesktop.layout.GroupLayout jPanel10Layout = new org.jdesktop.layout.GroupLayout(jPanel10);
        jPanel10.setLayout(jPanel10Layout);
        jPanel10Layout.setHorizontalGroup(
            jPanel10Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(org.jdesktop.layout.GroupLayout.TRAILING, jPanel10Layout.createSequentialGroup()
                .addContainerGap()
                .add(tb_RowMetadataTableName, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, 241, Short.MAX_VALUE))
        );
        jPanel10Layout.setVerticalGroup(
            jPanel10Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(tb_RowMetadataTableName, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
        );

        org.jdesktop.layout.GroupLayout jPanel7Layout = new org.jdesktop.layout.GroupLayout(jPanel7);
        jPanel7.setLayout(jPanel7Layout);
        jPanel7Layout.setHorizontalGroup(
            jPanel7Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel7Layout.createSequentialGroup()
                .addContainerGap()
                .add(jPanel7Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
                    .add(btn_IncludeRowColumn, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, 51, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                    .add(btn_RemoveRowColumn, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, 51, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE))
                .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED)
                .add(jScrollPane3, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, 196, Short.MAX_VALUE))
            .add(jPanel10, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
        );
        jPanel7Layout.setVerticalGroup(
            jPanel7Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel7Layout.createSequentialGroup()
                .add(jPanel10, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                .add(jPanel7Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
                    .add(jPanel7Layout.createSequentialGroup()
                        .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED)
                        .add(jScrollPane3, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, 253, Short.MAX_VALUE))
                    .add(jPanel7Layout.createSequentialGroup()
                        .add(56, 56, 56)
                        .add(btn_IncludeRowColumn)
                        .addPreferredGap(org.jdesktop.layout.LayoutStyle.UNRELATED)
                        .add(btn_RemoveRowColumn)
                        .addContainerGap())))
        );

        org.jdesktop.layout.GroupLayout jPanel1Layout = new org.jdesktop.layout.GroupLayout(jPanel1);
        jPanel1.setLayout(jPanel1Layout);
        jPanel1Layout.setHorizontalGroup(
            jPanel1Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel1Layout.createSequentialGroup()
                .add(jPanel1Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
                    .add(jPanel1Layout.createSequentialGroup()
                        .addContainerGap()
                        .add(jPanel2, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                        .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED)
                        .add(jPanel1Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.TRAILING)
                            .add(jPanel4, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                            .add(jPanel3, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE))
                        .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED)
                        .add(jPanel1Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
                            .add(jPanel6, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                            .add(jPanel5, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)))
                    .add(jPanel1Layout.createSequentialGroup()
                        .add(317, 317, 317)
                        .add(jLabel2)))
                .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED)
                .add(jPanel1Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
                    .add(cb_CreateRowmetadata)
                    .add(jPanel7, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
                .addContainerGap())
        );
        jPanel1Layout.setVerticalGroup(
            jPanel1Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel1Layout.createSequentialGroup()
                .add(20, 20, 20)
                .add(jLabel2)
                .add(18, 18, 18)
                .add(jPanel1Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
                    .add(jPanel2, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                    .add(jPanel1Layout.createSequentialGroup()
                        .add(jPanel1Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.TRAILING)
                            .add(cb_CreateRowmetadata)
                            .add(jPanel5, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE))
                        .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED)
                        .add(jPanel1Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
                            .add(jPanel6, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                            .add(jPanel4, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                            .add(org.jdesktop.layout.GroupLayout.TRAILING, jPanel7, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)))
                    .add(jPanel3, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)))
        );

        jPanel8.setName("jPanel8"); // NOI18N

        jScrollPane4.setName("jScrollPane4"); // NOI18N

        tbl_File.setBorder(javax.swing.BorderFactory.createLineBorder(new java.awt.Color(0, 0, 0)));
        tbl_File.setModel(new javax.swing.table.DefaultTableModel(
            new Object [][] {
                {null, null, null, null},
                {null, null, null, null},
                {null, null, null, null},
                {null, null, null, null}
            },
            new String [] {
                "Title 1", "Title 2", "Title 3", "Title 4"
            }
        ));
        tbl_File.setName("tbl_File"); // NOI18N
        tbl_File.setRowSelectionAllowed(false);
        jScrollPane4.setViewportView(tbl_File);

        org.jdesktop.layout.GroupLayout jPanel8Layout = new org.jdesktop.layout.GroupLayout(jPanel8);
        jPanel8.setLayout(jPanel8Layout);
        jPanel8Layout.setHorizontalGroup(
            jPanel8Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel8Layout.createSequentialGroup()
                .add(jScrollPane4, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, 931, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );
        jPanel8Layout.setVerticalGroup(
            jPanel8Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel8Layout.createSequentialGroup()
                .add(jScrollPane4, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, 166, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );

        jPanel9.setName("jPanel9"); // NOI18N

        btn_OK.setText(resourceMap.getString("btn_OK.text")); // NOI18N
        btn_OK.setName("btn_OK"); // NOI18N
        btn_OK.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                btn_OKActionPerformed(evt);
            }
        });

        btn_Cancel.setText(resourceMap.getString("btn_Cancel.text")); // NOI18N
        btn_Cancel.setName("btn_Cancel"); // NOI18N
        btn_Cancel.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                btn_CancelActionPerformed(evt);
            }
        });

        org.jdesktop.layout.GroupLayout jPanel9Layout = new org.jdesktop.layout.GroupLayout(jPanel9);
        jPanel9.setLayout(jPanel9Layout);
        jPanel9Layout.setHorizontalGroup(
            jPanel9Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(org.jdesktop.layout.GroupLayout.TRAILING, jPanel9Layout.createSequentialGroup()
                .addContainerGap(790, Short.MAX_VALUE)
                .add(btn_OK)
                .addPreferredGap(org.jdesktop.layout.LayoutStyle.UNRELATED)
                .add(btn_Cancel))
        );
        jPanel9Layout.setVerticalGroup(
            jPanel9Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel9Layout.createSequentialGroup()
                .addContainerGap(org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                .add(jPanel9Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.BASELINE)
                    .add(btn_Cancel)
                    .add(btn_OK)))
        );

        org.jdesktop.layout.GroupLayout layout = new org.jdesktop.layout.GroupLayout(getContentPane());
        getContentPane().setLayout(layout);
        layout.setHorizontalGroup(
            layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(layout.createSequentialGroup()
                .add(layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
                    .add(layout.createParallelGroup(org.jdesktop.layout.GroupLayout.TRAILING, false)
                        .add(org.jdesktop.layout.GroupLayout.LEADING, jPanel9, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                        .add(org.jdesktop.layout.GroupLayout.LEADING, jPanel8, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
                    .add(jPanel1, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE))
                .addContainerGap(org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );
        layout.setVerticalGroup(
            layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(layout.createSequentialGroup()
                .add(jPanel1, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(org.jdesktop.layout.LayoutStyle.UNRELATED)
                .add(jPanel8, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, 175, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED)
                .add(jPanel9, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );

        pack();
    }// </editor-fold>//GEN-END:initComponents

    private void rb_DataFileActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_rb_DataFileActionPerformed
        this.cmb_UniqueRowID.setEnabled(false);        
        this.btn_IncludeDataColumn.setEnabled(true);
        this.btn_RemoveDataColumn.setEnabled(true);
        this.lb_FieldsToInclude.setEnabled(true);
        this.cb_CreateRowmetadata.setEnabled(true);
        
        if (this.cb_CreateRowmetadata.isSelected()) {
            this.tb_RowMetadataTableName.setEnabled(true);
            this.btn_IncludeRowColumn.setEnabled(true);
            this.btn_RemoveRowColumn.setEnabled(true);
            this.lb_RowMetadataFields.setEnabled(true);
        }
    }//GEN-LAST:event_rb_DataFileActionPerformed

    private void rb_ColMetadataActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_rb_ColMetadataActionPerformed
        this.cmb_UniqueRowID.setEnabled(false);        
        this.btn_IncludeDataColumn.setEnabled(true);
        this.btn_RemoveDataColumn.setEnabled(true);
        this.lb_FieldsToInclude.setEnabled(true);
        this.btn_IncludeRowColumn.setEnabled(false);
        this.btn_RemoveRowColumn.setEnabled(false);
        this.tb_RowMetadataTableName.setEnabled(false);
        this.lb_RowMetadataFields.setEnabled(false);
        this.cb_CreateRowmetadata.setSelected(false);
        this.cb_CreateRowmetadata.setEnabled(false);
    }//GEN-LAST:event_rb_ColMetadataActionPerformed

    private void rb_RowMetadataActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_rb_RowMetadataActionPerformed
        this.cmb_UniqueRowID.setEnabled(false);
        this.btn_IncludeDataColumn.setEnabled(false);
        this.btn_RemoveDataColumn.setEnabled(false);
        this.lb_FieldsToInclude.setEnabled(false);
        this.btn_IncludeRowColumn.setEnabled(true);
        this.btn_RemoveRowColumn.setEnabled(true);
        this.tb_RowMetadataTableName.setEnabled(true);
        this.lb_RowMetadataFields.setEnabled(true);
        this.cb_CreateRowmetadata.setEnabled(true);
        this.cb_CreateRowmetadata.setSelected(true);
    }//GEN-LAST:event_rb_RowMetadataActionPerformed

    private void btn_CancelActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_btn_CancelActionPerformed
        this.setVisible(false);
    }//GEN-LAST:event_btn_CancelActionPerformed

    private void btn_IncludeDataColumnActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_btn_IncludeDataColumnActionPerformed
//        int[] i_SelectedIndices = lb_Fields.getSelectedIndices();
        Object[] s_SelectedValues = lb_Fields.getSelectedValues();
        
        for (int i = 0; i < s_SelectedValues.length; i++) {
            listToIncludeModel.addElement(s_SelectedValues[i]);
        }
        
        for (int i = 0; i < s_SelectedValues.length; i++) {
            listModel.removeElement(s_SelectedValues[i]);
        }
        
    }//GEN-LAST:event_btn_IncludeDataColumnActionPerformed

    private void btn_RemoveDataColumnActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_btn_RemoveDataColumnActionPerformed
        
        Object[] s_SelectedValues = lb_FieldsToInclude.getSelectedValues();
        
        for (int i = 0; i < s_SelectedValues.length; i++) {
            listModel.addElement(s_SelectedValues[i]);
        }
        
        for (int i = 0; i < s_SelectedValues.length; i++) {
            listToIncludeModel.removeElement(s_SelectedValues[i]);
        }
    }//GEN-LAST:event_btn_RemoveDataColumnActionPerformed

    private void btn_IncludeRowColumnActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_btn_IncludeRowColumnActionPerformed
        Object[] s_SelectedValues = lb_Fields.getSelectedValues();
        
        for (int i = 0; i < s_SelectedValues.length; i++) {
            listRowModel.addElement(s_SelectedValues[i]);
        }
        
        for (int i = 0; i < s_SelectedValues.length; i++) {
            listModel.removeElement(s_SelectedValues[i]);
        }
    }//GEN-LAST:event_btn_IncludeRowColumnActionPerformed

    private void btn_RemoveRowColumnActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_btn_RemoveRowColumnActionPerformed
        
        Object[] s_SelectedValues = this.lb_RowMetadataFields.getSelectedValues();
        
        for (int i = 0; i < s_SelectedValues.length; i++) {
            listModel.addElement(s_SelectedValues[i]);
        }
        
        for (int i = 0; i < s_SelectedValues.length; i++) {
            listRowModel.removeElement(s_SelectedValues[i]);
        }
    }//GEN-LAST:event_btn_RemoveRowColumnActionPerformed

    private void btn_OKActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_btn_OKActionPerformed
        
        // First Check with Radio Button is selected, and process from there
        if (this.rb_DataFile.isSelected()) {
            // Check that the listboxes have values
            if (this.lb_FieldsToInclude.getModel().getSize() < 1) {
                JOptionPane.showMessageDialog(this, "You must select " +
                        "columns to import", "ERROR: No columns selected",
                        JOptionPane.ERROR_MESSAGE);
            }
            else
            {
                DanteImportTableHandler dith = this.FillTableHandler();
                dith.setDataTableImport(true);
                dith.setColumnMetadataImport(false);
                dith.setRowMetadataImport(false);
                
                controller.ImportDataTableFromFile(dith);
                this.setVisible(false);
            }
        } else if (this.rb_ColMetadata.isSelected()) {
            // Check that the listboxes have values
            if (this.lb_FieldsToInclude.getModel().getSize() < 1) {
                JOptionPane.showMessageDialog(this, "You must select " +
                        "columns to import", "ERROR: No columns selected",
                        JOptionPane.ERROR_MESSAGE);
            }
            else 
            {
                DanteImportTableHandler dith = this.FillTableHandler();
                dith.setDataTableImport(false);
                dith.setColumnMetadataImport(true);
                dith.setRowMetadataImport(false);
                
                controller.ImportDataTableFromFile(dith);
                this.setVisible(false);
            }
        } else if (this.rb_RowMetadata.isSelected()) {
            // Check that the listboxes have values
            if (this.lb_RowMetadataFields.getModel().getSize() < 1) {
                JOptionPane.showMessageDialog(this, "You must select " +
                        "columns to import", "ERROR: No columns selected",
                        JOptionPane.ERROR_MESSAGE);
            }
            else 
            {
                DanteImportTableHandler dith = this.FillTableHandler();
                dith.setDataTableImport(false);
                dith.setColumnMetadataImport(false);
                dith.setRowMetadataImport(true);
                
                controller.ImportDataTableFromFile(dith);
                this.setVisible(false);
            }
        }
    }//GEN-LAST:event_btn_OKActionPerformed

    private void cb_CreateRowmetadataActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_cb_CreateRowmetadataActionPerformed
        
        if (this.cb_CreateRowmetadata.isSelected()) {
            this.tb_RowMetadataTableName.setEnabled(true);
            this.btn_IncludeRowColumn.setEnabled(true);
            this.btn_RemoveRowColumn.setEnabled(true);
            this.lb_RowMetadataFields.setEnabled(true);
        } else {
            this.tb_RowMetadataTableName.setEnabled(false);
            this.btn_IncludeRowColumn.setEnabled(false);
            this.btn_RemoveRowColumn.setEnabled(false);
            this.lb_RowMetadataFields.setEnabled(false);
        }
    }//GEN-LAST:event_cb_CreateRowmetadataActionPerformed

    /**
     * @param args the command line arguments
     */
    public static void main(String args[]) {
        java.awt.EventQueue.invokeLater(new Runnable() {

            public void run() {
                new DataImportDialog().setVisible(true);
            }
        });
    }
    // Variables declaration - do not modify//GEN-BEGIN:variables
    private javax.swing.JButton btn_Cancel;
    private javax.swing.JButton btn_IncludeDataColumn;
    private javax.swing.JButton btn_IncludeRowColumn;
    private javax.swing.JButton btn_OK;
    private javax.swing.JButton btn_RemoveDataColumn;
    private javax.swing.JButton btn_RemoveRowColumn;
    private javax.swing.ButtonGroup buttonGroup1;
    private javax.swing.JCheckBox cb_CreateRowmetadata;
    private javax.swing.JComboBox cmb_UniqueRowID;
    private javax.swing.JLabel jLabel2;
    private javax.swing.JPanel jPanel1;
    private javax.swing.JPanel jPanel10;
    private javax.swing.JPanel jPanel2;
    private javax.swing.JPanel jPanel3;
    private javax.swing.JPanel jPanel4;
    private javax.swing.JPanel jPanel5;
    private javax.swing.JPanel jPanel6;
    private javax.swing.JPanel jPanel7;
    private javax.swing.JPanel jPanel8;
    private javax.swing.JPanel jPanel9;
    private javax.swing.JScrollPane jScrollPane1;
    private javax.swing.JScrollPane jScrollPane2;
    private javax.swing.JScrollPane jScrollPane3;
    private javax.swing.JScrollPane jScrollPane4;
    private javax.swing.JList lb_Fields;
    private javax.swing.JList lb_FieldsToInclude;
    private javax.swing.JList lb_RowMetadataFields;
    private javax.swing.JRadioButton rb_ColMetadata;
    private javax.swing.JRadioButton rb_DataFile;
    private javax.swing.JRadioButton rb_RowMetadata;
    private javax.swing.JTextField tb_RowMetadataTableName;
    private javax.swing.JTextField tb_TableName;
    private javax.swing.JTable tbl_File;
    // End of variables declaration//GEN-END:variables
}

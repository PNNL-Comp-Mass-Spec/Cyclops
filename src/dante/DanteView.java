/*
 * DanteView.java
 */
package dante;

import dante.resources.DanteController;
import java.awt.Color;
import java.awt.Component;
import java.awt.Point;
import org.jdesktop.application.Action;
import org.jdesktop.application.ResourceMap;
import org.jdesktop.application.SingleFrameApplication;
import org.jdesktop.application.FrameView;
import org.jdesktop.application.TaskMonitor;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;
import java.awt.event.MouseListener;
import java.io.File;
import java.io.FileFilter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import javax.swing.Timer;
import javax.swing.Icon;
import javax.swing.JDialog;
import javax.swing.JFileChooser;
import javax.swing.JFrame;
import javax.swing.JMenuItem;
import javax.swing.JOptionPane;
import javax.swing.JPopupMenu;
import javax.swing.JTree;
import javax.swing.event.TreeSelectionEvent;
import javax.swing.event.TreeSelectionListener;
import javax.swing.table.DefaultTableModel;
import javax.swing.table.TableCellRenderer;
import javax.swing.tree.DefaultTreeModel;
import javax.swing.tree.TreePath;
import javax.swing.tree.TreeSelectionModel;

/**
 * The application's main frame.
 * @author  Joseph N. Brown
 * @date    6/3/2011
 */
public class DanteView extends FrameView{

    private DanteController dc;
//    private String s_ProjectName = "";
    private JTree navTree;
    private JPopupMenu navTreePopupMenu;

    public DanteView(SingleFrameApplication app) {
        super(app);
        dc = new DanteController(this);
        
        navTree = new JTree();
        navTreePopupMenu = new JPopupMenu();
        navTreePopupMenu.setInvoker(navTree);
        PopupHandler navTreeHandler = new PopupHandler(navTree, 
                navTreePopupMenu);
        navTreePopupMenu.add(getMenuItem("Open", navTreeHandler));
        navTreePopupMenu.add(getMenuItem("Rename", navTreeHandler));
        navTreePopupMenu.add(getMenuItem("Refresh", navTreeHandler));
        navTreePopupMenu.add(getMenuItem("Remove", navTreeHandler));
        
//        navTree.getSelectionModel().setSelectionMode(
//                TreeSelectionModel.SINGLE_TREE_SELECTION);
//        navTree.addTreeSelectionListener(this);
        
        MouseListener ml = new MouseAdapter() {
     
            public void mousePressed(MouseEvent e) {
         
                int selRow = navTree.getRowForLocation(e.getX(), e.getY());
         
                TreePath selPath = navTree.getPathForLocation(e.getX(), e.getY());
         
                if(selRow != -1) {
                    if(e.getClickCount() == 1) {
//                        mySingleClick(selRow, selPath);
                    }
                    else if(e.getClickCount() == 2) { // Double-click
//                        JOptionPane.showMessageDialog(new JFrame(), 
//                            "Selected Row: " + selRow + "\n" +
//                            "Selected Path: " + selPath);
                        
                        ArrayList<String> s_Obj = dc.GetObjectsInWorkspace();
                        Object[] o_Path = selPath.getPath();
                        if (o_Path.length == 1) { // user selected root node
                            // Do nothing, just return
                            return;
                        }
                        String[] s_StrPath = new String[o_Path.length];
                        for (int i = 0; i < s_StrPath.length; i++) {
                            s_StrPath[i] = o_Path[i].toString();
                        }
                        System.out.println("There are " + s_Obj.size() + " objects.");
                        for(int i = 0; i < s_Obj.size(); i++) {
                            System.out.println(s_Obj.get(i));
                        }
                        
                        System.out.println("Datasets:");
                        for (int i = 0; i < s_StrPath.length; i++) { 
                            System.out.println(s_StrPath[i]);
                        }
                        String s_Dataset2Display = "";
                        
                        System.out.println("Objects in Path:");
                        for (int i = s_StrPath.length-1; i >= 0; i--) {
                            if (s_Obj.contains(s_StrPath[i])) {
                                s_Dataset2Display = s_StrPath[i];
                                System.out.println("FOUND OBJECT: " + s_Dataset2Display);
                                break;
                            }
                        }
                        
                        try {
                            System.out.println("Displaying Table: " + s_Dataset2Display);
                            tbl_DataValues.setModel(dc.GetDataset(s_Dataset2Display));
                        } catch (Exception exc) {
                            JOptionPane.showMessageDialog(new JFrame(), 
                                    "Error opening table:\n" + exc.toString(), 
                                    "Problem creating DefaultTableModel", 
                                    JOptionPane.ERROR_MESSAGE);
                        }
                    }
                }
            }
        };
        navTree.addMouseListener(ml);
        

        initComponents();

        // status bar initialization - message timeout, idle icon and busy animation, etc
        ResourceMap resourceMap = getResourceMap();
        int messageTimeout = resourceMap.getInteger("StatusBar.messageTimeout");
        messageTimer = new Timer(messageTimeout, new ActionListener() {

            public void actionPerformed(ActionEvent e) {
                statusMessageLabel.setText("");
            }
        });
        messageTimer.setRepeats(false);
        int busyAnimationRate = resourceMap.getInteger("StatusBar.busyAnimationRate");
        for (int i = 0; i < busyIcons.length; i++) {
            busyIcons[i] = resourceMap.getIcon("StatusBar.busyIcons[" + i + "]");
        }
        busyIconTimer = new Timer(busyAnimationRate, new ActionListener() {

            public void actionPerformed(ActionEvent e) {
                busyIconIndex = (busyIconIndex + 1) % busyIcons.length;
                statusAnimationLabel.setIcon(busyIcons[busyIconIndex]);
            }
        });
        idleIcon = resourceMap.getIcon("StatusBar.idleIcon");
        statusAnimationLabel.setIcon(idleIcon);
        progressBar.setVisible(false);

        // connecting action tasks to status bar via TaskMonitor
        TaskMonitor taskMonitor = new TaskMonitor(getApplication().getContext());
        taskMonitor.addPropertyChangeListener(new java.beans.PropertyChangeListener() {

            public void propertyChange(java.beans.PropertyChangeEvent evt) {
                String propertyName = evt.getPropertyName();
                if ("started".equals(propertyName)) {
                    if (!busyIconTimer.isRunning()) {
                        statusAnimationLabel.setIcon(busyIcons[0]);
                        busyIconIndex = 0;
                        busyIconTimer.start();
                    }
                    progressBar.setVisible(true);
                    progressBar.setIndeterminate(true);
                } else if ("done".equals(propertyName)) {
                    busyIconTimer.stop();
                    statusAnimationLabel.setIcon(idleIcon);
                    progressBar.setVisible(false);
                    progressBar.setValue(0);
                } else if ("message".equals(propertyName)) {
                    String text = (String) (evt.getNewValue());
                    statusMessageLabel.setText((text == null) ? "" : text);
                    messageTimer.restart();
                } else if ("progress".equals(propertyName)) {
                    int value = (Integer) (evt.getNewValue());
                    progressBar.setVisible(true);
                    progressBar.setIndeterminate(false);
                    progressBar.setValue(value);
                }
            }
        });
        
        this.tbl_Datasets.setModel(new DefaultTableModel());
        this.tbl_DataValues.setModel(new DefaultTableModel());
        this.navTree.setModel(null);
    }
    
    private JMenuItem getMenuItem(String s, ActionListener al) {
        JMenuItem menuItem = new JMenuItem(s);
        menuItem.setActionCommand(s.toUpperCase());
        menuItem.addActionListener(al);
        return menuItem;
    }
    
    public void valueChanged(TreeSelectionEvent e) {
        
        
    }

    @Action
    public void showAboutBox() {
        if (aboutBox == null) {
            JFrame mainFrame = DanteApp.getApplication().getMainFrame();
            aboutBox = new DanteAboutBox(mainFrame);
            aboutBox.setLocationRelativeTo(mainFrame);
        }
        DanteApp.getApplication().show(aboutBox);
    }
    
    public void RefreshTreeDatasets() {
        navTree.setModel(dc.RefreshTreeDatasets());
    }

    public void RefreshTableDatasets() {
        tbl_Datasets.setModel(dc.RefreshTableDatasets());
    }

    /** This method is called from within the constructor to
     * initialize the form.
     * WARNING: Do NOT modify this code. The content of this method is
     * always regenerated by the Form Editor.
     */
    @SuppressWarnings("unchecked")
    // <editor-fold defaultstate="collapsed" desc="Generated Code">//GEN-BEGIN:initComponents
    private void initComponents() {

        mainPanel = new javax.swing.JPanel();
        jSplitPane1 = new javax.swing.JSplitPane();
        jPanel1 = new javax.swing.JPanel();
        jPanel3 = new javax.swing.JPanel();
        jComboBox1 = new javax.swing.JComboBox();
        jSplitPane2 = new javax.swing.JSplitPane();
        sp_Navigator = new javax.swing.JScrollPane(navTree);
        jScrollPane1 = new javax.swing.JScrollPane();
        tbl_Datasets = new javax.swing.JTable();
        jPanel2 = new javax.swing.JPanel();
        jTabbedPane1 = new javax.swing.JTabbedPane();
        jScrollPane2 = new javax.swing.JScrollPane();
        tbl_DataValues = new javax.swing.JTable() {
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
        menuBar = new javax.swing.JMenuBar();
        javax.swing.JMenu fileMenu = new javax.swing.JMenu();
        importFileMenuItem = new javax.swing.JMenuItem();
        jSeparator2 = new javax.swing.JPopupMenu.Separator();
        openMenuItem = new javax.swing.JMenuItem();
        saveProjectMenuItem = new javax.swing.JMenuItem();
        saveProjectAsMenuItem = new javax.swing.JMenuItem();
        closeProjectMenuItem = new javax.swing.JMenuItem();
        jSeparator1 = new javax.swing.JPopupMenu.Separator();
        exportTableMenuItem = new javax.swing.JMenuItem();
        jSeparator16 = new javax.swing.JPopupMenu.Separator();
        javax.swing.JMenuItem exitMenuItem = new javax.swing.JMenuItem();
        dataMenu = new javax.swing.JMenu();
        tableDisplayOptionsMenuItem = new javax.swing.JMenuItem();
        jSeparator3 = new javax.swing.JPopupMenu.Separator();
        mergeMenuItem = new javax.swing.JMenuItem();
        filterMenuItem = new javax.swing.JMenuItem();
        jMenuItem5 = new javax.swing.JMenuItem();
        jSeparator4 = new javax.swing.JPopupMenu.Separator();
        sortMenuItem = new javax.swing.JMenuItem();
        editFactorsMenuItem = new javax.swing.JMenuItem();
        insertColMenuItem = new javax.swing.JMenuItem();
        insertRowMenuItem = new javax.swing.JMenuItem();
        jSeparator17 = new javax.swing.JPopupMenu.Separator();
        summaryTableMenuItem = new javax.swing.JMenuItem();
        metadataMenu = new javax.swing.JMenu();
        defineFactorsMenuItem = new javax.swing.JMenuItem();
        defineRowMetadataMenuItem = new javax.swing.JMenuItem();
        jSeparator5 = new javax.swing.JPopupMenu.Separator();
        linkMetadataMenuItem = new javax.swing.JMenuItem();
        showLinksMenuItem = new javax.swing.JMenuItem();
        jSeparator6 = new javax.swing.JPopupMenu.Separator();
        createAliasesMenuItem = new javax.swing.JMenuItem();
        applyAliasesMenuItem = new javax.swing.JMenuItem();
        preProcessMenu = new javax.swing.JMenu();
        jMenuItem3 = new javax.swing.JMenuItem();
        jMenuItem4 = new javax.swing.JMenuItem();
        NormalizationMenu = new javax.swing.JMenu();
        linearRegressionMenuItem = new javax.swing.JMenuItem();
        eigenMSMenuItem = new javax.swing.JMenuItem();
        loessMenuItem = new javax.swing.JMenuItem();
        dataQuantilesMenuItem = new javax.swing.JMenuItem();
        imputationMenu = new javax.swing.JMenu();
        imputeMenuItem = new javax.swing.JMenuItem();
        modelBasedImputeMenuItem = new javax.swing.JMenuItem();
        rollupMenu = new javax.swing.JMenu();
        rRollupMenuItem = new javax.swing.JMenuItem();
        qRollupMenuItem = new javax.swing.JMenuItem();
        zRollupMenuItem = new javax.swing.JMenuItem();
        statisticsMenu = new javax.swing.JMenu();
        anovaMenuItem = new javax.swing.JMenuItem();
        calcFoldChangeMenuItem = new javax.swing.JMenuItem();
        jSeparator7 = new javax.swing.JPopupMenu.Separator();
        shapiroWilksMenuItem = new javax.swing.JMenuItem();
        kruskalWalisMenuItem = new javax.swing.JMenuItem();
        fisherCountMenuItem = new javax.swing.JMenuItem();
        jSeparator8 = new javax.swing.JPopupMenu.Separator();
        pValAdjMenuItem = new javax.swing.JMenuItem();
        jSeparator9 = new javax.swing.JPopupMenu.Separator();
        splitSignUpDownMenuItem = new javax.swing.JMenuItem();
        plotMenu = new javax.swing.JMenu();
        newPlotWindowMenuItem = new javax.swing.JMenuItem();
        switchActiveWindowMenuItem = new javax.swing.JMenuItem();
        plotOptionsMenuItem = new javax.swing.JMenuItem();
        jSeparator10 = new javax.swing.JPopupMenu.Separator();
        matrixScatterMenuItem = new javax.swing.JMenuItem();
        plot3dMenuItem = new javax.swing.JMenuItem();
        jSeparator11 = new javax.swing.JPopupMenu.Separator();
        histogramMenuItem = new javax.swing.JMenuItem();
        boxPlotMenuItem = new javax.swing.JMenuItem();
        qqPlotMenuItem = new javax.swing.JMenuItem();
        jSeparator12 = new javax.swing.JPopupMenu.Separator();
        correlationHeatmapMenuItem = new javax.swing.JMenuItem();
        correlationEllipsesMenuItem = new javax.swing.JMenuItem();
        jSeparator13 = new javax.swing.JPopupMenu.Separator();
        vennDiagramMenuItem = new javax.swing.JMenuItem();
        jSeparator14 = new javax.swing.JPopupMenu.Separator();
        volcanoPlotMenuItem = new javax.swing.JMenuItem();
        plotAgainstRowMeanMenuItem = new javax.swing.JMenuItem();
        exploreMenu = new javax.swing.JMenu();
        clusteringMenuItem = new javax.swing.JMenuItem();
        patternSearchMenuItem = new javax.swing.JMenuItem();
        jSeparator15 = new javax.swing.JPopupMenu.Separator();
        pcaPlotMenuItem = new javax.swing.JMenuItem();
        dynamicRowPlotMenuItem = new javax.swing.JMenuItem();
        javax.swing.JMenu helpMenu = new javax.swing.JMenu();
        javax.swing.JMenuItem aboutMenuItem = new javax.swing.JMenuItem();
        jSeparator18 = new javax.swing.JPopupMenu.Separator();
        jMenuItem1 = new javax.swing.JMenuItem();
        testingMenuItem = new javax.swing.JMenuItem();
        testIrisMenuItem = new javax.swing.JMenuItem();
        statusPanel = new javax.swing.JPanel();
        statusMessageLabel = new javax.swing.JLabel();
        statusAnimationLabel = new javax.swing.JLabel();
        progressBar = new javax.swing.JProgressBar();
        openFileChooser = new javax.swing.JFileChooser();
        openProjectChooser = new javax.swing.JFileChooser();
        saveProjectFileChooser = new javax.swing.JFileChooser();

        mainPanel.setBorder(javax.swing.BorderFactory.createLineBorder(new java.awt.Color(0, 0, 0)));
        mainPanel.setName("mainPanel"); // NOI18N

        jSplitPane1.setDividerLocation(250);
        jSplitPane1.setName("jSplitPane1"); // NOI18N

        jPanel1.setName("jPanel1"); // NOI18N

        org.jdesktop.application.ResourceMap resourceMap = org.jdesktop.application.Application.getInstance(dante.DanteApp.class).getContext().getResourceMap(DanteView.class);
        jPanel3.setBorder(javax.swing.BorderFactory.createTitledBorder(resourceMap.getString("jPanel3.border.title"))); // NOI18N
        jPanel3.setName("jPanel3"); // NOI18N

        jComboBox1.setModel(new javax.swing.DefaultComboBoxModel(new String[] { "Datasets", "Datasets and Models", "Plots", "All" }));
        jComboBox1.setName("jComboBox1"); // NOI18N

        org.jdesktop.layout.GroupLayout jPanel3Layout = new org.jdesktop.layout.GroupLayout(jPanel3);
        jPanel3.setLayout(jPanel3Layout);
        jPanel3Layout.setHorizontalGroup(
            jPanel3Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jComboBox1, 0, 236, Short.MAX_VALUE)
        );
        jPanel3Layout.setVerticalGroup(
            jPanel3Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel3Layout.createSequentialGroup()
                .add(jComboBox1, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(2, Short.MAX_VALUE))
        );

        jSplitPane2.setOrientation(javax.swing.JSplitPane.VERTICAL_SPLIT);
        jSplitPane2.setName("jSplitPane2"); // NOI18N

        sp_Navigator.setName("sp_Navigator"); // NOI18N
        jSplitPane2.setBottomComponent(sp_Navigator);

        jScrollPane1.setName("jScrollPane1"); // NOI18N

        tbl_Datasets.setModel(new javax.swing.table.DefaultTableModel(
            new Object [][] {
                {null, null, null, null},
                {null, null, null, null},
                {null, null, null, null},
                {null, null, null, null}
            },
            new String [] {
                "Name", "Class", "Rows", "Columns"
            }
        ));
        tbl_Datasets.setName("tbl_Datasets"); // NOI18N
        jScrollPane1.setViewportView(tbl_Datasets);

        jSplitPane2.setLeftComponent(jScrollPane1);

        org.jdesktop.layout.GroupLayout jPanel1Layout = new org.jdesktop.layout.GroupLayout(jPanel1);
        jPanel1.setLayout(jPanel1Layout);
        jPanel1Layout.setHorizontalGroup(
            jPanel1Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jSplitPane2, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, 248, Short.MAX_VALUE)
            .add(jPanel3, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
        );
        jPanel1Layout.setVerticalGroup(
            jPanel1Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jPanel1Layout.createSequentialGroup()
                .add(jPanel3, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, 57, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED)
                .add(jSplitPane2, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, 622, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );

        jSplitPane1.setLeftComponent(jPanel1);

        jPanel2.setName("jPanel2"); // NOI18N

        jTabbedPane1.setName("jTabbedPane1"); // NOI18N

        jScrollPane2.setName("jScrollPane2"); // NOI18N

        tbl_DataValues.setModel(new javax.swing.table.DefaultTableModel(
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
        tbl_DataValues.setName("tbl_DataValues"); // NOI18N
        jScrollPane2.setViewportView(tbl_DataValues);

        jTabbedPane1.addTab(resourceMap.getString("jScrollPane2.TabConstraints.tabTitle"), jScrollPane2); // NOI18N

        org.jdesktop.layout.GroupLayout jPanel2Layout = new org.jdesktop.layout.GroupLayout(jPanel2);
        jPanel2.setLayout(jPanel2Layout);
        jPanel2Layout.setHorizontalGroup(
            jPanel2Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(org.jdesktop.layout.GroupLayout.TRAILING, jTabbedPane1, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, 821, Short.MAX_VALUE)
        );
        jPanel2Layout.setVerticalGroup(
            jPanel2Layout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(jTabbedPane1, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, 675, Short.MAX_VALUE)
        );

        jSplitPane1.setRightComponent(jPanel2);

        org.jdesktop.layout.GroupLayout mainPanelLayout = new org.jdesktop.layout.GroupLayout(mainPanel);
        mainPanel.setLayout(mainPanelLayout);
        mainPanelLayout.setHorizontalGroup(
            mainPanelLayout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(mainPanelLayout.createSequentialGroup()
                .add(20, 20, 20)
                .add(jSplitPane1, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, 1082, Short.MAX_VALUE)
                .add(32, 32, 32))
        );
        mainPanelLayout.setVerticalGroup(
            mainPanelLayout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(mainPanelLayout.createSequentialGroup()
                .addContainerGap()
                .add(jSplitPane1, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, 679, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                .addContainerGap(org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE))
        );

        menuBar.setName("menuBar"); // NOI18N

        fileMenu.setText(resourceMap.getString("fileMenu.text")); // NOI18N
        fileMenu.setName("fileMenu"); // NOI18N

        importFileMenuItem.setAccelerator(javax.swing.KeyStroke.getKeyStroke(java.awt.event.KeyEvent.VK_O, java.awt.event.InputEvent.META_MASK));
        importFileMenuItem.setText(resourceMap.getString("openFileMenuItem.text")); // NOI18N
        importFileMenuItem.setName("openFileMenuItem"); // NOI18N
        importFileMenuItem.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                openFileActionPerformed(evt);
            }
        });
        fileMenu.add(importFileMenuItem);

        jSeparator2.setName("jSeparator2"); // NOI18N
        fileMenu.add(jSeparator2);

        openMenuItem.setAccelerator(javax.swing.KeyStroke.getKeyStroke(java.awt.event.KeyEvent.VK_O, java.awt.event.InputEvent.SHIFT_MASK | java.awt.event.InputEvent.META_MASK));
        openMenuItem.setText(resourceMap.getString("openProjectMenuItem.text")); // NOI18N
        openMenuItem.setName("openProjectMenuItem"); // NOI18N
        openMenuItem.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                openProjectMenuItemActionPerformed(evt);
            }
        });
        fileMenu.add(openMenuItem);

        saveProjectMenuItem.setAccelerator(javax.swing.KeyStroke.getKeyStroke(java.awt.event.KeyEvent.VK_S, java.awt.event.InputEvent.META_MASK));
        saveProjectMenuItem.setText(resourceMap.getString("saveProjectMenuItem.text")); // NOI18N
        saveProjectMenuItem.setName("saveProjectMenuItem"); // NOI18N
        saveProjectMenuItem.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                saveProjectMenuItemActionPerformed(evt);
            }
        });
        fileMenu.add(saveProjectMenuItem);

        saveProjectAsMenuItem.setAccelerator(javax.swing.KeyStroke.getKeyStroke(java.awt.event.KeyEvent.VK_S, java.awt.event.InputEvent.SHIFT_MASK | java.awt.event.InputEvent.META_MASK));
        saveProjectAsMenuItem.setText(resourceMap.getString("saveProjectAsMenuItem.text")); // NOI18N
        saveProjectAsMenuItem.setName("saveProjectAsMenuItem"); // NOI18N
        saveProjectAsMenuItem.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                saveProjectAsMenuItemActionPerformed(evt);
            }
        });
        fileMenu.add(saveProjectAsMenuItem);

        closeProjectMenuItem.setAccelerator(javax.swing.KeyStroke.getKeyStroke(java.awt.event.KeyEvent.VK_W, java.awt.event.InputEvent.META_MASK));
        closeProjectMenuItem.setText(resourceMap.getString("closeProjectMenuItem.text")); // NOI18N
        closeProjectMenuItem.setName("closeProjectMenuItem"); // NOI18N
        closeProjectMenuItem.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                closeProjectMenuItemActionPerformed(evt);
            }
        });
        fileMenu.add(closeProjectMenuItem);

        jSeparator1.setName("jSeparator1"); // NOI18N
        fileMenu.add(jSeparator1);

        exportTableMenuItem.setText(resourceMap.getString("exportTableMenuItem.text")); // NOI18N
        exportTableMenuItem.setName("exportTableMenuItem"); // NOI18N
        fileMenu.add(exportTableMenuItem);

        jSeparator16.setName("jSeparator16"); // NOI18N
        fileMenu.add(jSeparator16);

        javax.swing.ActionMap actionMap = org.jdesktop.application.Application.getInstance(dante.DanteApp.class).getContext().getActionMap(DanteView.class, this);
        exitMenuItem.setAction(actionMap.get("quit")); // NOI18N
        exitMenuItem.setName("exitMenuItem"); // NOI18N
        fileMenu.add(exitMenuItem);

        menuBar.add(fileMenu);

        dataMenu.setText(resourceMap.getString("dataMenu.text")); // NOI18N
        dataMenu.setName("dataMenu"); // NOI18N

        tableDisplayOptionsMenuItem.setText(resourceMap.getString("tableDisplayOptionsMenuItem.text")); // NOI18N
        tableDisplayOptionsMenuItem.setName("tableDisplayOptionsMenuItem"); // NOI18N
        dataMenu.add(tableDisplayOptionsMenuItem);

        jSeparator3.setName("jSeparator3"); // NOI18N
        dataMenu.add(jSeparator3);

        mergeMenuItem.setText(resourceMap.getString("mergeMenuItem.text")); // NOI18N
        mergeMenuItem.setName("mergeMenuItem"); // NOI18N
        dataMenu.add(mergeMenuItem);

        filterMenuItem.setText(resourceMap.getString("filterMenuItem.text")); // NOI18N
        filterMenuItem.setName("filterMenuItem"); // NOI18N
        dataMenu.add(filterMenuItem);

        jMenuItem5.setText(resourceMap.getString("aggregateMenuItem.text")); // NOI18N
        jMenuItem5.setName("aggregateMenuItem"); // NOI18N
        dataMenu.add(jMenuItem5);

        jSeparator4.setName("jSeparator4"); // NOI18N
        dataMenu.add(jSeparator4);

        sortMenuItem.setText(resourceMap.getString("sortMenuItem.text")); // NOI18N
        sortMenuItem.setName("sortMenuItem"); // NOI18N
        dataMenu.add(sortMenuItem);

        editFactorsMenuItem.setText(resourceMap.getString("editFactorsMenuItem.text")); // NOI18N
        editFactorsMenuItem.setName("editFactorsMenuItem"); // NOI18N
        dataMenu.add(editFactorsMenuItem);

        insertColMenuItem.setText(resourceMap.getString("insertColMenuItem.text")); // NOI18N
        insertColMenuItem.setName("insertColMenuItem"); // NOI18N
        dataMenu.add(insertColMenuItem);

        insertRowMenuItem.setText(resourceMap.getString("insertRowMenuItem.text")); // NOI18N
        insertRowMenuItem.setName("insertRowMenuItem"); // NOI18N
        dataMenu.add(insertRowMenuItem);

        jSeparator17.setName("jSeparator17"); // NOI18N
        dataMenu.add(jSeparator17);

        summaryTableMenuItem.setText(resourceMap.getString("summaryTableMenuItem.text")); // NOI18N
        summaryTableMenuItem.setName("summaryTableMenuItem"); // NOI18N
        dataMenu.add(summaryTableMenuItem);

        menuBar.add(dataMenu);

        metadataMenu.setText(resourceMap.getString("metadataMenu.text")); // NOI18N
        metadataMenu.setName("metadataMenu"); // NOI18N

        defineFactorsMenuItem.setText(resourceMap.getString("defineFactorsMenuItem.text")); // NOI18N
        defineFactorsMenuItem.setName("defineFactorsMenuItem"); // NOI18N
        metadataMenu.add(defineFactorsMenuItem);

        defineRowMetadataMenuItem.setText(resourceMap.getString("defineRowMetadataMenuItem.text")); // NOI18N
        defineRowMetadataMenuItem.setName("defineRowMetadataMenuItem"); // NOI18N
        metadataMenu.add(defineRowMetadataMenuItem);

        jSeparator5.setName("jSeparator5"); // NOI18N
        metadataMenu.add(jSeparator5);

        linkMetadataMenuItem.setText(resourceMap.getString("linkMetadataMenuItem.text")); // NOI18N
        linkMetadataMenuItem.setName("linkMetadataMenuItem"); // NOI18N
        metadataMenu.add(linkMetadataMenuItem);

        showLinksMenuItem.setText(resourceMap.getString("showLinksMenuItem.text")); // NOI18N
        showLinksMenuItem.setName("showLinksMenuItem"); // NOI18N
        metadataMenu.add(showLinksMenuItem);

        jSeparator6.setName("jSeparator6"); // NOI18N
        metadataMenu.add(jSeparator6);

        createAliasesMenuItem.setText(resourceMap.getString("createAliasesMenuItem.text")); // NOI18N
        createAliasesMenuItem.setName("createAliasesMenuItem"); // NOI18N
        metadataMenu.add(createAliasesMenuItem);

        applyAliasesMenuItem.setText(resourceMap.getString("applyAliasesMenuItem.text")); // NOI18N
        applyAliasesMenuItem.setName("applyAliasesMenuItem"); // NOI18N
        metadataMenu.add(applyAliasesMenuItem);

        menuBar.add(metadataMenu);

        preProcessMenu.setText(resourceMap.getString("preProcessMenu.text")); // NOI18N
        preProcessMenu.setName("preProcessMenu"); // NOI18N

        jMenuItem3.setText(resourceMap.getString("transformMenuItem.text")); // NOI18N
        jMenuItem3.setName("transformMenuItem"); // NOI18N
        preProcessMenu.add(jMenuItem3);

        jMenuItem4.setText(resourceMap.getString("mbFilterMenuItem.text")); // NOI18N
        jMenuItem4.setName("mbFilterMenuItem"); // NOI18N
        preProcessMenu.add(jMenuItem4);

        NormalizationMenu.setText(resourceMap.getString("NormalizationMenu.text")); // NOI18N
        NormalizationMenu.setName("NormalizationMenu"); // NOI18N

        linearRegressionMenuItem.setText(resourceMap.getString("linearRegressionMenuItem.text")); // NOI18N
        linearRegressionMenuItem.setName("linearRegressionMenuItem"); // NOI18N
        NormalizationMenu.add(linearRegressionMenuItem);

        eigenMSMenuItem.setText(resourceMap.getString("eigenMSMenuItem.text")); // NOI18N
        eigenMSMenuItem.setName("eigenMSMenuItem"); // NOI18N
        NormalizationMenu.add(eigenMSMenuItem);

        loessMenuItem.setText(resourceMap.getString("loessMenuItem.text")); // NOI18N
        loessMenuItem.setName("loessMenuItem"); // NOI18N
        NormalizationMenu.add(loessMenuItem);

        dataQuantilesMenuItem.setText(resourceMap.getString("dataQuantilesMenuItem.text")); // NOI18N
        dataQuantilesMenuItem.setName("dataQuantilesMenuItem"); // NOI18N
        NormalizationMenu.add(dataQuantilesMenuItem);

        preProcessMenu.add(NormalizationMenu);

        imputationMenu.setText(resourceMap.getString("imputationMenu.text")); // NOI18N
        imputationMenu.setName("imputationMenu"); // NOI18N

        imputeMenuItem.setText(resourceMap.getString("imputeMenuItem.text")); // NOI18N
        imputeMenuItem.setName("imputeMenuItem"); // NOI18N
        imputationMenu.add(imputeMenuItem);

        modelBasedImputeMenuItem.setText(resourceMap.getString("modelBasedImputeMenuItem.text")); // NOI18N
        modelBasedImputeMenuItem.setName("modelBasedImputeMenuItem"); // NOI18N
        imputationMenu.add(modelBasedImputeMenuItem);

        preProcessMenu.add(imputationMenu);

        menuBar.add(preProcessMenu);

        rollupMenu.setText(resourceMap.getString("rollupMenu.text")); // NOI18N
        rollupMenu.setName("rollupMenu"); // NOI18N

        rRollupMenuItem.setText(resourceMap.getString("rRollupMenuItem.text")); // NOI18N
        rRollupMenuItem.setName("rRollupMenuItem"); // NOI18N
        rollupMenu.add(rRollupMenuItem);

        qRollupMenuItem.setText(resourceMap.getString("qRollupMenuItem.text")); // NOI18N
        qRollupMenuItem.setName("qRollupMenuItem"); // NOI18N
        rollupMenu.add(qRollupMenuItem);

        zRollupMenuItem.setText(resourceMap.getString("zRollupMenuItem.text")); // NOI18N
        zRollupMenuItem.setName("zRollupMenuItem"); // NOI18N
        rollupMenu.add(zRollupMenuItem);

        menuBar.add(rollupMenu);

        statisticsMenu.setText(resourceMap.getString("statisticsMenu.text")); // NOI18N
        statisticsMenu.setName("statisticsMenu"); // NOI18N

        anovaMenuItem.setText(resourceMap.getString("anovaMenuItem.text")); // NOI18N
        anovaMenuItem.setName("anovaMenuItem"); // NOI18N
        statisticsMenu.add(anovaMenuItem);

        calcFoldChangeMenuItem.setText(resourceMap.getString("calcFoldChangeMenuItem.text")); // NOI18N
        calcFoldChangeMenuItem.setName("calcFoldChangeMenuItem"); // NOI18N
        statisticsMenu.add(calcFoldChangeMenuItem);

        jSeparator7.setName("jSeparator7"); // NOI18N
        statisticsMenu.add(jSeparator7);

        shapiroWilksMenuItem.setText(resourceMap.getString("shapiroWilksMenuItem.text")); // NOI18N
        shapiroWilksMenuItem.setName("shapiroWilksMenuItem"); // NOI18N
        statisticsMenu.add(shapiroWilksMenuItem);

        kruskalWalisMenuItem.setText(resourceMap.getString("kruskalWalisMenuItem.text")); // NOI18N
        kruskalWalisMenuItem.setName("kruskalWalisMenuItem"); // NOI18N
        statisticsMenu.add(kruskalWalisMenuItem);

        fisherCountMenuItem.setText(resourceMap.getString("fisherCountMenuItem.text")); // NOI18N
        fisherCountMenuItem.setName("fisherCountMenuItem"); // NOI18N
        statisticsMenu.add(fisherCountMenuItem);

        jSeparator8.setName("jSeparator8"); // NOI18N
        statisticsMenu.add(jSeparator8);

        pValAdjMenuItem.setText(resourceMap.getString("pValAdjMenuItem.text")); // NOI18N
        pValAdjMenuItem.setName("pValAdjMenuItem"); // NOI18N
        statisticsMenu.add(pValAdjMenuItem);

        jSeparator9.setName("jSeparator9"); // NOI18N
        statisticsMenu.add(jSeparator9);

        splitSignUpDownMenuItem.setText(resourceMap.getString("splitSignUpDownMenuItem.text")); // NOI18N
        splitSignUpDownMenuItem.setName("splitSignUpDownMenuItem"); // NOI18N
        statisticsMenu.add(splitSignUpDownMenuItem);

        menuBar.add(statisticsMenu);

        plotMenu.setText(resourceMap.getString("plotMenu.text")); // NOI18N
        plotMenu.setName("plotMenu"); // NOI18N

        newPlotWindowMenuItem.setText(resourceMap.getString("newPlotWindowMenuItem.text")); // NOI18N
        newPlotWindowMenuItem.setName("newPlotWindowMenuItem"); // NOI18N
        plotMenu.add(newPlotWindowMenuItem);

        switchActiveWindowMenuItem.setText(resourceMap.getString("switchActiveWindowMenuItem.text")); // NOI18N
        switchActiveWindowMenuItem.setName("switchActiveWindowMenuItem"); // NOI18N
        plotMenu.add(switchActiveWindowMenuItem);

        plotOptionsMenuItem.setText(resourceMap.getString("plotOptionsMenuItem.text")); // NOI18N
        plotOptionsMenuItem.setName("plotOptionsMenuItem"); // NOI18N
        plotMenu.add(plotOptionsMenuItem);

        jSeparator10.setName("jSeparator10"); // NOI18N
        plotMenu.add(jSeparator10);

        matrixScatterMenuItem.setText(resourceMap.getString("matrixScatterMenuItem.text")); // NOI18N
        matrixScatterMenuItem.setName("matrixScatterMenuItem"); // NOI18N
        plotMenu.add(matrixScatterMenuItem);

        plot3dMenuItem.setText(resourceMap.getString("plot3dMenuItem.text")); // NOI18N
        plot3dMenuItem.setName("plot3dMenuItem"); // NOI18N
        plotMenu.add(plot3dMenuItem);

        jSeparator11.setName("jSeparator11"); // NOI18N
        plotMenu.add(jSeparator11);

        histogramMenuItem.setText(resourceMap.getString("histogramMenuItem.text")); // NOI18N
        histogramMenuItem.setName("histogramMenuItem"); // NOI18N
        plotMenu.add(histogramMenuItem);

        boxPlotMenuItem.setText(resourceMap.getString("boxPlotMenuItem.text")); // NOI18N
        boxPlotMenuItem.setName("boxPlotMenuItem"); // NOI18N
        plotMenu.add(boxPlotMenuItem);

        qqPlotMenuItem.setText(resourceMap.getString("qqPlotMenuItem.text")); // NOI18N
        qqPlotMenuItem.setName("qqPlotMenuItem"); // NOI18N
        plotMenu.add(qqPlotMenuItem);

        jSeparator12.setName("jSeparator12"); // NOI18N
        plotMenu.add(jSeparator12);

        correlationHeatmapMenuItem.setText(resourceMap.getString("correlationHeatmapMenuItem.text")); // NOI18N
        correlationHeatmapMenuItem.setName("correlationHeatmapMenuItem"); // NOI18N
        plotMenu.add(correlationHeatmapMenuItem);

        correlationEllipsesMenuItem.setText(resourceMap.getString("correlationEllipsesMenuItem.text")); // NOI18N
        correlationEllipsesMenuItem.setName("correlationEllipsesMenuItem"); // NOI18N
        plotMenu.add(correlationEllipsesMenuItem);

        jSeparator13.setName("jSeparator13"); // NOI18N
        plotMenu.add(jSeparator13);

        vennDiagramMenuItem.setText(resourceMap.getString("vennDiagramMenuItem.text")); // NOI18N
        vennDiagramMenuItem.setName("vennDiagramMenuItem"); // NOI18N
        plotMenu.add(vennDiagramMenuItem);

        jSeparator14.setName("jSeparator14"); // NOI18N
        plotMenu.add(jSeparator14);

        volcanoPlotMenuItem.setText(resourceMap.getString("volcanoPlotMenuItem.text")); // NOI18N
        volcanoPlotMenuItem.setName("volcanoPlotMenuItem"); // NOI18N
        plotMenu.add(volcanoPlotMenuItem);

        plotAgainstRowMeanMenuItem.setText(resourceMap.getString("plotAgainstRowMeanMenuItem.text")); // NOI18N
        plotAgainstRowMeanMenuItem.setName("plotAgainstRowMeanMenuItem"); // NOI18N
        plotMenu.add(plotAgainstRowMeanMenuItem);

        menuBar.add(plotMenu);

        exploreMenu.setText(resourceMap.getString("exploreMenu.text")); // NOI18N
        exploreMenu.setName("exploreMenu"); // NOI18N

        clusteringMenuItem.setText(resourceMap.getString("clusteringMenuItem.text")); // NOI18N
        clusteringMenuItem.setName("clusteringMenuItem"); // NOI18N
        exploreMenu.add(clusteringMenuItem);

        patternSearchMenuItem.setText(resourceMap.getString("patternSearchMenuItem.text")); // NOI18N
        patternSearchMenuItem.setName("patternSearchMenuItem"); // NOI18N
        exploreMenu.add(patternSearchMenuItem);

        jSeparator15.setName("jSeparator15"); // NOI18N
        exploreMenu.add(jSeparator15);

        pcaPlotMenuItem.setText(resourceMap.getString("pcaPlotMenuItem.text")); // NOI18N
        pcaPlotMenuItem.setName("pcaPlotMenuItem"); // NOI18N
        exploreMenu.add(pcaPlotMenuItem);

        dynamicRowPlotMenuItem.setText(resourceMap.getString("dynamicRowPlotMenuItem.text")); // NOI18N
        dynamicRowPlotMenuItem.setName("dynamicRowPlotMenuItem"); // NOI18N
        exploreMenu.add(dynamicRowPlotMenuItem);

        menuBar.add(exploreMenu);

        helpMenu.setText(resourceMap.getString("helpMenu.text")); // NOI18N
        helpMenu.setName("helpMenu"); // NOI18N

        aboutMenuItem.setAction(actionMap.get("showAboutBox")); // NOI18N
        aboutMenuItem.setName("aboutMenuItem"); // NOI18N
        helpMenu.add(aboutMenuItem);

        jSeparator18.setName("jSeparator18"); // NOI18N
        helpMenu.add(jSeparator18);

        jMenuItem1.setText(resourceMap.getString("jMenuItem1.text")); // NOI18N
        jMenuItem1.setName("jMenuItem1"); // NOI18N
        helpMenu.add(jMenuItem1);

        testingMenuItem.setAccelerator(javax.swing.KeyStroke.getKeyStroke(java.awt.event.KeyEvent.VK_T, java.awt.event.InputEvent.META_MASK));
        testingMenuItem.setText(resourceMap.getString("testingMenuItem.text")); // NOI18N
        testingMenuItem.setName("testingMenuItem"); // NOI18N
        testingMenuItem.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                testingMenuItemActionPerformed(evt);
            }
        });
        helpMenu.add(testingMenuItem);

        testIrisMenuItem.setAccelerator(javax.swing.KeyStroke.getKeyStroke(java.awt.event.KeyEvent.VK_M, java.awt.event.InputEvent.META_MASK));
        testIrisMenuItem.setText(resourceMap.getString("testIrisMenuItem.text")); // NOI18N
        testIrisMenuItem.setName("testIrisMenuItem"); // NOI18N
        testIrisMenuItem.addActionListener(new java.awt.event.ActionListener() {
            public void actionPerformed(java.awt.event.ActionEvent evt) {
                testIrisMenuItemActionPerformed(evt);
            }
        });
        helpMenu.add(testIrisMenuItem);

        menuBar.add(helpMenu);

        statusPanel.setName("statusPanel"); // NOI18N

        statusMessageLabel.setName("statusMessageLabel"); // NOI18N

        statusAnimationLabel.setHorizontalAlignment(javax.swing.SwingConstants.LEFT);
        statusAnimationLabel.setName("statusAnimationLabel"); // NOI18N

        progressBar.setName("progressBar"); // NOI18N

        org.jdesktop.layout.GroupLayout statusPanelLayout = new org.jdesktop.layout.GroupLayout(statusPanel);
        statusPanel.setLayout(statusPanelLayout);
        statusPanelLayout.setHorizontalGroup(
            statusPanelLayout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(statusPanelLayout.createSequentialGroup()
                .addContainerGap()
                .add(statusMessageLabel)
                .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED, 940, Short.MAX_VALUE)
                .add(progressBar, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE)
                .addPreferredGap(org.jdesktop.layout.LayoutStyle.RELATED)
                .add(statusAnimationLabel)
                .addContainerGap())
        );
        statusPanelLayout.setVerticalGroup(
            statusPanelLayout.createParallelGroup(org.jdesktop.layout.GroupLayout.LEADING)
            .add(org.jdesktop.layout.GroupLayout.TRAILING, statusPanelLayout.createSequentialGroup()
                .addContainerGap(org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
                .add(statusPanelLayout.createParallelGroup(org.jdesktop.layout.GroupLayout.BASELINE)
                    .add(statusMessageLabel)
                    .add(statusAnimationLabel)
                    .add(progressBar, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE, org.jdesktop.layout.GroupLayout.DEFAULT_SIZE, org.jdesktop.layout.GroupLayout.PREFERRED_SIZE))
                .add(3, 3, 3))
        );

        openFileChooser.setDialogTitle(resourceMap.getString("openFileChooser.dialogTitle")); // NOI18N
        openFileChooser.setName("openFileChooser"); // NOI18N

        openProjectChooser.setDialogTitle(resourceMap.getString("openProjectChooser.dialogTitle")); // NOI18N
        openProjectChooser.setName("openProjectChooser"); // NOI18N

        saveProjectFileChooser.setDialogTitle(resourceMap.getString("saveProjectFileChooser.dialogTitle")); // NOI18N
        saveProjectFileChooser.setDialogType(javax.swing.JFileChooser.SAVE_DIALOG);
        saveProjectFileChooser.setName("saveProjectFileChooser"); // NOI18N

        setComponent(mainPanel);
        setMenuBar(menuBar);
        setStatusBar(statusPanel);
    }// </editor-fold>//GEN-END:initComponents

    /// User select to import a file
    private void openFileActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_openFileActionPerformed
        // User selected "Open File"

        // Add filters to the FileChoosers
        openFileChooser.addChoosableFileFilter(new TextFileFilter());
        openFileChooser.addChoosableFileFilter(new CSVFileFilter());
        openFileChooser.addChoosableFileFilter(new SQLite1FileFilter());
        openFileChooser.addChoosableFileFilter(new SQLite2FileFilter());
        openFileChooser.setFileFilter(new TextFileFilter());

        int returnVal = openFileChooser.showDialog(mainPanel, null);
        if (returnVal == JFileChooser.APPROVE_OPTION) {
            File file = openFileChooser.getSelectedFile();
            try {
                /// READ THE FILE
                //JOptionPane.showMessageDialog(mainPanel, file.getAbsoluteFile());
                dc.openFile(file.getAbsoluteFile().toString());
            } catch (Exception e) {
                JOptionPane.showMessageDialog(mainPanel, e.toString(),
                        "Error Reading File", JOptionPane.ERROR_MESSAGE);
            }
        } else {
            System.out.println("File access cancelled by user.");
        }
    }//GEN-LAST:event_openFileActionPerformed

    /// User selected to open a project (RData file)
    private void openProjectMenuItemActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_openProjectMenuItemActionPerformed
        // User selected to open project

        openProjectChooser.addChoosableFileFilter(new RDataFileFilter());

        int returnVal = openProjectChooser.showDialog(mainPanel, null);
        if (returnVal == JFileChooser.APPROVE_OPTION) {
            File file = openProjectChooser.getSelectedFile();
            try {
                /// READ THE FILE
//                JOptionPane.showMessageDialog(mainPanel, file.getAbsoluteFile());
                dc.loadProject(file.getAbsoluteFile().toString());
                this.tbl_Datasets.setModel(dc.RefreshTableDatasets());
                this.navTree.setModel(dc.RefreshTreeDatasets());
            } catch (Exception e) {
                JOptionPane.showMessageDialog(mainPanel, e.toString(),
                        "Error Reading File", JOptionPane.ERROR_MESSAGE);
            }
        } else {
            System.out.println("File access cancelled by user.");
        }
    }//GEN-LAST:event_openProjectMenuItemActionPerformed

    private void saveProjectAsMenuItemActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_saveProjectAsMenuItemActionPerformed
        this.saveProjectFileChooser.addChoosableFileFilter(new RDataFileFilter());

        int returnVal = saveProjectFileChooser.showDialog(mainPanel, null);
        if (returnVal == JFileChooser.APPROVE_OPTION) {
            File file = saveProjectFileChooser.getSelectedFile();
            try {
                /// SAVE THE PROJECT NAME
//                dc.saveProject(dc.getWorkspaceName());
                dc.setWorkspaceName(file.getAbsolutePath());
                dc.SaveWorkspace(dc.getWorkspaceName());

            } catch (Exception e) {
                JOptionPane.showMessageDialog(mainPanel, e.toString(),
                        "Error Saving File", JOptionPane.ERROR_MESSAGE);
            }
        }
    }//GEN-LAST:event_saveProjectAsMenuItemActionPerformed

    private void saveProjectMenuItemActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_saveProjectMenuItemActionPerformed
        if (dc.getWorkspaceName() == null) {
            this.saveProjectFileChooser.addChoosableFileFilter(new RDataFileFilter());

            int returnVal = saveProjectFileChooser.showDialog(mainPanel, null);
            if (returnVal == JFileChooser.APPROVE_OPTION) {
                File file = saveProjectFileChooser.getSelectedFile();
                try {
                    /// SAVE THE PROJECT NAME
//                  dc.saveProject(dc.getWorkspaceName());
                    dc.setWorkspaceName(file.getAbsolutePath());
                    dc.SaveWorkspace(dc.getWorkspaceName());

                } catch (Exception e) {
                    JOptionPane.showMessageDialog(mainPanel, e.toString(),
                            "Error Saving File", JOptionPane.ERROR_MESSAGE);
                }
            }
        } else {
            dc.SaveWorkspace(dc.getWorkspaceName());
        }
    }//GEN-LAST:event_saveProjectMenuItemActionPerformed

    private void testingMenuItemActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_testingMenuItemActionPerformed
        // TODO add your handling code here:
        String s_TestProject = "/Users/d3x050/Documents/SysVirol_Integration_Manuscript/ProteomicDatasets/test.RData";
        dc.loadProject(s_TestProject);
        RefreshTableDatasets();
        RefreshTreeDatasets();
    }//GEN-LAST:event_testingMenuItemActionPerformed

    private void testIrisMenuItemActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_testIrisMenuItemActionPerformed
        dc.testIrisData();
        RefreshTableDatasets();
        RefreshTreeDatasets();
    }//GEN-LAST:event_testIrisMenuItemActionPerformed

    private void closeProjectMenuItemActionPerformed(java.awt.event.ActionEvent evt) {//GEN-FIRST:event_closeProjectMenuItemActionPerformed
        dc.CloseWorkspace();
        this.RefreshTableDatasets();
        this.navTree.setModel(null);
        this.tbl_DataValues.setModel(new DefaultTableModel());
    }//GEN-LAST:event_closeProjectMenuItemActionPerformed

    // Variables declaration - do not modify//GEN-BEGIN:variables
    private javax.swing.JMenu NormalizationMenu;
    private javax.swing.JMenuItem anovaMenuItem;
    private javax.swing.JMenuItem applyAliasesMenuItem;
    private javax.swing.JMenuItem boxPlotMenuItem;
    private javax.swing.JMenuItem calcFoldChangeMenuItem;
    private javax.swing.JMenuItem closeProjectMenuItem;
    private javax.swing.JMenuItem clusteringMenuItem;
    private javax.swing.JMenuItem correlationEllipsesMenuItem;
    private javax.swing.JMenuItem correlationHeatmapMenuItem;
    private javax.swing.JMenuItem createAliasesMenuItem;
    private javax.swing.JMenu dataMenu;
    private javax.swing.JMenuItem dataQuantilesMenuItem;
    private javax.swing.JMenuItem defineFactorsMenuItem;
    private javax.swing.JMenuItem defineRowMetadataMenuItem;
    private javax.swing.JMenuItem dynamicRowPlotMenuItem;
    private javax.swing.JMenuItem editFactorsMenuItem;
    private javax.swing.JMenuItem eigenMSMenuItem;
    private javax.swing.JMenu exploreMenu;
    private javax.swing.JMenuItem exportTableMenuItem;
    private javax.swing.JMenuItem filterMenuItem;
    private javax.swing.JMenuItem fisherCountMenuItem;
    private javax.swing.JMenuItem histogramMenuItem;
    private javax.swing.JMenuItem importFileMenuItem;
    private javax.swing.JMenu imputationMenu;
    private javax.swing.JMenuItem imputeMenuItem;
    private javax.swing.JMenuItem insertColMenuItem;
    private javax.swing.JMenuItem insertRowMenuItem;
    private javax.swing.JComboBox jComboBox1;
    private javax.swing.JMenuItem jMenuItem1;
    private javax.swing.JMenuItem jMenuItem3;
    private javax.swing.JMenuItem jMenuItem4;
    private javax.swing.JMenuItem jMenuItem5;
    private javax.swing.JPanel jPanel1;
    private javax.swing.JPanel jPanel2;
    private javax.swing.JPanel jPanel3;
    private javax.swing.JScrollPane jScrollPane1;
    private javax.swing.JScrollPane jScrollPane2;
    private javax.swing.JPopupMenu.Separator jSeparator1;
    private javax.swing.JPopupMenu.Separator jSeparator10;
    private javax.swing.JPopupMenu.Separator jSeparator11;
    private javax.swing.JPopupMenu.Separator jSeparator12;
    private javax.swing.JPopupMenu.Separator jSeparator13;
    private javax.swing.JPopupMenu.Separator jSeparator14;
    private javax.swing.JPopupMenu.Separator jSeparator15;
    private javax.swing.JPopupMenu.Separator jSeparator16;
    private javax.swing.JPopupMenu.Separator jSeparator17;
    private javax.swing.JPopupMenu.Separator jSeparator18;
    private javax.swing.JPopupMenu.Separator jSeparator2;
    private javax.swing.JPopupMenu.Separator jSeparator3;
    private javax.swing.JPopupMenu.Separator jSeparator4;
    private javax.swing.JPopupMenu.Separator jSeparator5;
    private javax.swing.JPopupMenu.Separator jSeparator6;
    private javax.swing.JPopupMenu.Separator jSeparator7;
    private javax.swing.JPopupMenu.Separator jSeparator8;
    private javax.swing.JPopupMenu.Separator jSeparator9;
    private javax.swing.JSplitPane jSplitPane1;
    private javax.swing.JSplitPane jSplitPane2;
    private javax.swing.JTabbedPane jTabbedPane1;
    private javax.swing.JMenuItem kruskalWalisMenuItem;
    private javax.swing.JMenuItem linearRegressionMenuItem;
    private javax.swing.JMenuItem linkMetadataMenuItem;
    private javax.swing.JMenuItem loessMenuItem;
    private javax.swing.JPanel mainPanel;
    private javax.swing.JMenuItem matrixScatterMenuItem;
    private javax.swing.JMenuBar menuBar;
    private javax.swing.JMenuItem mergeMenuItem;
    private javax.swing.JMenu metadataMenu;
    private javax.swing.JMenuItem modelBasedImputeMenuItem;
    private javax.swing.JMenuItem newPlotWindowMenuItem;
    private javax.swing.JFileChooser openFileChooser;
    private javax.swing.JMenuItem openMenuItem;
    private javax.swing.JFileChooser openProjectChooser;
    private javax.swing.JMenuItem pValAdjMenuItem;
    private javax.swing.JMenuItem patternSearchMenuItem;
    private javax.swing.JMenuItem pcaPlotMenuItem;
    private javax.swing.JMenuItem plot3dMenuItem;
    private javax.swing.JMenuItem plotAgainstRowMeanMenuItem;
    private javax.swing.JMenu plotMenu;
    private javax.swing.JMenuItem plotOptionsMenuItem;
    private javax.swing.JMenu preProcessMenu;
    private javax.swing.JProgressBar progressBar;
    private javax.swing.JMenuItem qRollupMenuItem;
    private javax.swing.JMenuItem qqPlotMenuItem;
    private javax.swing.JMenuItem rRollupMenuItem;
    private javax.swing.JMenu rollupMenu;
    private javax.swing.JMenuItem saveProjectAsMenuItem;
    private javax.swing.JFileChooser saveProjectFileChooser;
    private javax.swing.JMenuItem saveProjectMenuItem;
    private javax.swing.JMenuItem shapiroWilksMenuItem;
    private javax.swing.JMenuItem showLinksMenuItem;
    private javax.swing.JMenuItem sortMenuItem;
    private javax.swing.JScrollPane sp_Navigator;
    private javax.swing.JMenuItem splitSignUpDownMenuItem;
    private javax.swing.JMenu statisticsMenu;
    private javax.swing.JLabel statusAnimationLabel;
    private javax.swing.JLabel statusMessageLabel;
    private javax.swing.JPanel statusPanel;
    private javax.swing.JMenuItem summaryTableMenuItem;
    private javax.swing.JMenuItem switchActiveWindowMenuItem;
    private javax.swing.JMenuItem tableDisplayOptionsMenuItem;
    private javax.swing.JTable tbl_DataValues;
    private javax.swing.JTable tbl_Datasets;
    private javax.swing.JMenuItem testIrisMenuItem;
    private javax.swing.JMenuItem testingMenuItem;
    private javax.swing.JMenuItem vennDiagramMenuItem;
    private javax.swing.JMenuItem volcanoPlotMenuItem;
    private javax.swing.JMenuItem zRollupMenuItem;
    // End of variables declaration//GEN-END:variables
    private final Timer messageTimer;
    private final Timer busyIconTimer;
    private final Icon idleIcon;
    private final Icon[] busyIcons = new Icon[15];
    private int busyIconIndex = 0;
    private JDialog aboutBox;
}

class TextFileFilter extends javax.swing.filechooser.FileFilter {

    @Override
    public boolean accept(File file) {
        // Allow only directories, or files with ".txt" extension

        return file.isDirectory() || file.getAbsolutePath().endsWith(".txt");
    }

    @Override
    public String getDescription() {
        // This description will be displayed in the dialog,
        // hard-coded = ugly, should be done via I18N
        return "Text documents (*.txt)";
    }
}

class CSVFileFilter extends javax.swing.filechooser.FileFilter {

    @Override
    public boolean accept(File file) {
        // Allow only directories, or files with ".txt" extension

        return file.isDirectory() || file.getAbsolutePath().endsWith(".csv");
    }

    @Override
    public String getDescription() {
        // This description will be displayed in the dialog,
        // hard-coded = ugly, should be done via I18N
        return "CSV documents (*.csv)";
    }
}

class SQLite1FileFilter extends javax.swing.filechooser.FileFilter {

    @Override
    public boolean accept(File file) {
        // Allow only directories, or files with ".txt" extension

        return file.isDirectory() || file.getAbsolutePath().endsWith(".db");
    }

    @Override
    public String getDescription() {
        // This description will be displayed in the dialog,
        // hard-coded = ugly, should be done via I18N
        return "SQLite documents (*.db)";
    }
}

class SQLite2FileFilter extends javax.swing.filechooser.FileFilter {

    @Override
    public boolean accept(File file) {
        // Allow only directories, or files with ".txt" extension

        return file.isDirectory() || file.getAbsolutePath().endsWith(".db3");
    }

    @Override
    public String getDescription() {
        // This description will be displayed in the dialog,
        // hard-coded = ugly, should be done via I18N
        return "SQLite documents (*.db3)";
    }
}

class RDataFileFilter extends javax.swing.filechooser.FileFilter {

    @Override
    public boolean accept(File file) {
        // Allow only directories, or files with ".txt" extension

        return file.isDirectory() || file.getAbsolutePath().endsWith(".RData");
    }

    @Override
    public String getDescription() {
        // This description will be displayed in the dialog,
        // hard-coded = ugly, should be done via I18N
        return "R Workspace documents (*.RData)";
    }
}

/// Class that handles the popup menu events on the Tree
class PopupHandler implements ActionListener {
    JTree tree;
    JPopupMenu popup;
    Point loc;

    public PopupHandler(JTree tree, JPopupMenu popup) {
        this.tree = tree;
        this.popup = popup;
        tree.addMouseListener(ma);
    }

    public void actionPerformed(ActionEvent e) {
        String ac = e.getActionCommand();
        TreePath path  = tree.getPathForLocation(loc.x, loc.y);
        //System.out.println("path = " + path);
        //System.out.printf("loc = [%d, %d]%n", loc.x, loc.y);
//        if(ac.equals("ADD CHILD"))
//            addChild(path);
//        if(ac.equals("ADD SIBLING"))
//            addSibling(path);
    }

//    private void addChild(TreePath path) {
//        DefaultMutableTreeNode parent =
//            (DefaultMutableTreeNode)path.getLastPathComponent();
//        int count = parent.getChildCount();
//        DefaultMutableTreeNode child =
//            new DefaultMutableTreeNode("child " + count);
//        DefaultTreeModel model = (DefaultTreeModel)tree.getModel();
//        model.insertNodeInto(child, parent, count);
//    }

//    private void addSibling(TreePath path) {
//        DefaultMutableTreeNode node =
//            (DefaultMutableTreeNode)path.getLastPathComponent();
//        DefaultMutableTreeNode parent =
//            (DefaultMutableTreeNode)node.getParent();
//        int count = parent.getChildCount();
//        DefaultMutableTreeNode child =
//            new DefaultMutableTreeNode("child " + count);
//        DefaultTreeModel model = (DefaultTreeModel)tree.getModel();
//        model.insertNodeInto(child, parent, count);
//    }

    private MouseListener ma = new MouseAdapter() {
        private void checkForPopup(MouseEvent e) {
            if(e.isPopupTrigger()) {
                loc = e.getPoint();
                popup.show(tree, loc.x, loc.y);
            }
        }

        public void mousePressed(MouseEvent e)  { checkForPopup(e); }
        public void mouseReleased(MouseEvent e) { checkForPopup(e); }
        public void mouseClicked(MouseEvent e)  { checkForPopup(e); }
    };
}
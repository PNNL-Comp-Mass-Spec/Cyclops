/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package dante.resources;

/**
 *
 * @author d3x050
 */
public class DanteImportTableHandler {

    private String FileName;
    private String TableName;
    private String RowMetadataTableName;

    /**
     * Get the value of RowMetadataTableName
     *
     * @return the value of RowMetadataTableName
     */
    public String getRowMetadataTableName() {
        return RowMetadataTableName;
    }

    /**
     * Set the value of RowMetadataTableName
     *
     * @param RowMetadataTableName new value of RowMetadataTableName
     */
    public void setRowMetadataTableName(String RowMetadataTableName) {
        this.RowMetadataTableName = RowMetadataTableName;
    }

    private String[] ColumnsToKeep;
    private String UniqueRowID;
    private boolean IncludeRowMetaData;
    private String[] RowMetadataColumns;
    private boolean DataTableImport;
    private boolean ColumnMetadataImport;
    private boolean RowMetadataImport;

    /**
     * Get the value of RowMetadataImport
     *
     * @return the value of RowMetadataImport
     */
    public boolean isRowMetadataImport() {
        return RowMetadataImport;
    }

    /**
     * Set the value of RowMetadataImport
     *
     * @param RowMetadataImport new value of RowMetadataImport
     */
    public void setRowMetadataImport(boolean RowMetadataImport) {
        this.RowMetadataImport = RowMetadataImport;
    }


    /**
     * Get the value of ColumnMetadataImport
     *
     * @return the value of ColumnMetadataImport
     */
    public boolean isColumnMetadataImport() {
        return ColumnMetadataImport;
    }

    /**
     * Set the value of ColumnMetadataImport
     *
     * @param ColumnMetadataImport new value of ColumnMetadataImport
     */
    public void setColumnMetadataImport(boolean ColumnMetadataImport) {
        this.ColumnMetadataImport = ColumnMetadataImport;
    }


    /**
     * Get the value of DataTableImport
     *
     * @return the value of DataTableImport
     */
    public boolean isDataTableImport() {
        return DataTableImport;
    }

    /**
     * Set the value of DataTableImport
     *
     * @param DataTableImport new value of DataTableImport
     */
    public void setDataTableImport(boolean DataTableImport) {
        this.DataTableImport = DataTableImport;
    }


    /**
     * Get the value of RowMetadataColumns
     *
     * @return the value of RowMetadataColumns
     */
    public String[] getRowMetadataColumns() {
        return RowMetadataColumns;
    }

    /**
     * Set the value of RowMetadataColumns
     *
     * @param RowMetadataColumns new value of RowMetadataColumns
     */
    public void setRowMetadataColumns(String[] RowMetadataColumns) {
        this.RowMetadataColumns = RowMetadataColumns;
    }


    /**
     * Get the value of IncludeRowMetaData
     *
     * @return the value of IncludeRowMetaData
     */
    public boolean isIncludeRowMetaData() {
        return IncludeRowMetaData;
    }

    /**
     * Set the value of IncludeRowMetaData
     *
     * @param IncludeRowMetaData new value of IncludeRowMetaData
     */
    public void setIncludeRowMetaData(boolean IncludeRowMetaData) {
        this.IncludeRowMetaData = IncludeRowMetaData;
    }


    /**
     * Get the value of UniqueRowID
     *
     * @return the value of UniqueRowID
     */
    public String getUniqueRowID() {
        return UniqueRowID;
    }

    /**
     * Set the value of UniqueRowID
     *
     * @param UniqueRowID new value of UniqueRowID
     */
    public void setUniqueRowID(String UniqueRowID) {
        this.UniqueRowID = UniqueRowID;
    }


    /**
     * Get the value of ColumnsToKeep
     *
     * @return the value of ColumnsToKeep
     */
    public String[] getColumnsToKeep() {
        return ColumnsToKeep;
    }

    /**
     * Set the value of ColumnsToKeep
     *
     * @param ColumnsToKeep new value of ColumnsToKeep
     */
    public void setColumnsToKeep(String[] ColumnsToKeep) {
        this.ColumnsToKeep = ColumnsToKeep;
    }


    /**
     * Get the value of TableName
     *
     * @return the value of TableName
     */
    public String getTableName() {
        return TableName;
    }

    /**
     * Set the value of TableName
     *
     * @param TableName new value of TableName
     */
    public void setTableName(String TableName) {
        this.TableName = TableName;
    }


    /**
     * Get the value of FileName
     *
     * @return the value of FileName
     */
    public String getFileName() {
        return FileName;
    }

    /**
     * Set the value of FileName
     *
     * @param FileName new value of FileName
     */
    public void setFileName(String FileName) {
        this.FileName = FileName;
    }

}

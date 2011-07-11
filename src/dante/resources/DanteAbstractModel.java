/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package dante.resources;

import java.beans.PropertyChangeListener;
import java.beans.PropertyChangeSupport;

/**
 *
 * @author  Joseph N. Brown
 * @date    6/3/2011
 */
public abstract class DanteAbstractModel {
    protected PropertyChangeSupport propertyChangeSupport;
    
    public DanteAbstractModel() {
        propertyChangeSupport = new PropertyChangeSupport(this);
    }
    
    public void addPropertyChangeListener(PropertyChangeListener listener) {
        propertyChangeSupport.addPropertyChangeListener(listener);
    }

    public void removePropertyChangeListener(PropertyChangeListener listener) {
        propertyChangeSupport.removePropertyChangeListener(listener);
    }

    protected void firePropertyChange(String propertyName, Object oldValue, Object newValue) {
        propertyChangeSupport.firePropertyChange(propertyName, oldValue, newValue);
    }
}

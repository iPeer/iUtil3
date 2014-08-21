package com.simple.ipeer.config;

/**
 *
 * @author iPeer
 */
public interface IConfigEntry {

    public boolean isProperty();
    public boolean isCategory();
    
    public String getName();
    public String name();
    public String getRegisteredName();
    
    public void setName(String name) throws UnsupportedConfigOperationException;
    
}

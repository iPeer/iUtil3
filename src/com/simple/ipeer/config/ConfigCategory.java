package com.simple.ipeer.config;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;

/**
 *
 * @author iPeer
 */
public class ConfigCategory {
    
    private String name;
    public Map<String, ConfigCategory> subCategories = new TreeMap<String, ConfigCategory>();
    public Map<String, Property> properties = new TreeMap<String, Property>();
    private String comment;
    private boolean hasComment = false;
    private ConfigCategory parent;
    
    public ConfigCategory(String name) throws UnsupportedConfigOperationException {
	this(name, null);
    }
    
    public ConfigCategory(String name, ConfigCategory parent) throws UnsupportedConfigOperationException {
	this.parent = parent;
	setName(name);
    }
    
    public String getName() {
	return this.name;
    }
    
    public String name() {
	return getName();
    }
    
    private ConfigCategory setParent(ConfigCategory c) {
	this.parent = c;
	return this;
    }
    
    public ConfigCategory getParent() { return this.parent; }
    
    public ConfigCategory addCategory(String name) throws UnsupportedConfigOperationException {
	return addSubCategory(name, this);
    }
    
    public ConfigCategory addSubCategory(String name) throws UnsupportedConfigOperationException {
	return addSubCategory(name, this);
    }
    
    public ConfigCategory addSubCategory(String name, ConfigCategory parent) throws UnsupportedConfigOperationException {
	ConfigCategory c = new ConfigCategory(name, parent);
	this.subCategories.put(name, c);
	return this;
    }
    
    public ConfigCategory addCategory(ConfigCategory cc) {
	return addSubCategory(cc, this);
    }
    
    public ConfigCategory addSubCategory(ConfigCategory cc) {
	return addSubCategory(cc, this);
    }
    
    public ConfigCategory addSubCategory(ConfigCategory cc, ConfigCategory parent) {
	cc.setParent(parent);
	this.subCategories.put(cc.getName(), cc);
	return this;
    }
    
    public ConfigCategory addProperty(Property p) {
	return setProperty(p.getName(), p);
    }
    
    public ConfigCategory addProperty(String name, Property p) {
	return setProperty(name, p);
    }
    
    public ConfigCategory setProperty(String name, Property p) {
	this.properties.put(name, p);
	return this;
    }
    
    public ConfigCategory addProperties(Property... props) {
	return setProperties(props);
    }
    
    public ConfigCategory setProperties(Property... props) {
	for (Property p : props)
	    addProperty(p.getName(), p);
	return this;
    }
    
    public ConfigCategory getCategory(String name) throws UnsupportedConfigOperationException {
	return getCategory(name, false);
    }
    
    public List<ConfigCategory> getSubCategoriesAsList() {
	List<ConfigCategory> cats = new ArrayList<ConfigCategory>(); // PROP HUN'EEEEEERS!
	HashMap<String, ConfigCategory> _tmp = new HashMap<String, ConfigCategory>();
	_tmp.putAll(this.subCategories);
	for (String k : _tmp.keySet())
	    cats.add(_tmp.get(k));
	return cats;
    }
    
    public ConfigCategory[] getSubCategories() {
	List<ConfigCategory> cats = getSubCategoriesAsList();
	ConfigCategory[] cList = new ConfigCategory[cats.size()];
	return getSubCategoriesAsList().toArray(cList);
    }
    
    public List<Property> getPropertiesAsList() {
	List<Property> props = new ArrayList<Property>(); // PROP HUN'EEEEEERS!
	HashMap<String, Property> _tmp = new HashMap<String, Property>();
	_tmp.putAll(this.properties);
	for (String k : _tmp.keySet())
	    props.add(_tmp.get(k));
	return props;
    }
    
    public Property[] getProperties() {
	List<Property> props = getPropertiesAsList();
	Property[] pList = new Property[props.size()];
	return getPropertiesAsList().toArray(pList);
    }
    
    public ConfigCategory getCategory(String name, boolean createIfNotExists) throws UnsupportedConfigOperationException {
	if (this.subCategories.containsKey(name))
	    return this.subCategories.get(name);
	else
	    if (!createIfNotExists)
		throw new UnsupportedConfigOperationException("Attempted to fetch a non-existant config category");
	    else {
		addSubCategory(name);
		return getCategory(name, false);
	    }
	
    }
    
    public String getRegisteredName() {
	if (getName().contains(" "))
	    return "\""+getName()+"\"";
	else
	    return getName();
    }
    
    public ConfigCategory setComment(String comment) {
	this.hasComment = true;
	this.comment = comment;
	return this;
    }
    
    public boolean hasComment() {
	return this.hasComment;
    }
    
    public String getComment() {
	return this.comment;
    }
    
    public void setName(String name) throws UnsupportedConfigOperationException {
	if (name.equals("") || name.length() < 1)
	    throw new UnsupportedConfigOperationException("Property name cannot be an empty, nulled, or zero-length string");
	this.name = name;
    }
    
    public void printStructure() {
	printStructure(this, 1);
    }
    
    public void printStructure(int i) {
	printStructure(this, i);
    }
    
    public int length() { return this.subCategories.size() + this.properties.size(); }
    
    public void printStructure(ConfigCategory cc, int i) {
	String str = "\t";
	String pre = new String(new char[i]).replace("\0", "\t");
	System.out.println(new String(new char[i - 1]).replace("\0", "\t")+"CATEGORY \""+cc.getName()+"\" ("+cc.length()+" entries)");
	i++;
	if (cc.getProperties().length > 0)
	    for (Property a : cc.getProperties())
		System.out.println(pre+"PROPERTY \""+a.getName()+"\": "+a.toString());
	if (cc.getSubCategories().length > 0)
	    for (ConfigCategory c : cc.getSubCategories())
		printStructure(c, i);
    }
    
}

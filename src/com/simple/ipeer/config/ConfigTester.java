package com.simple.ipeer.config;

/**
 *
 * @author iPeer
 *
 * This class is used during the coding of new features to the config and is now needed in final products.
 *
 */
public class ConfigTester {
    
    public ConfigTester() {
	
    }
    
    public static void main(String[] args) throws UnsupportedConfigOperationException {
	Configuration conf = new Configuration("test.cfg");
	ConfigCategory cfg = new ConfigTester().getTestConfig();
	conf.setStructure(cfg);
	conf.save();
	System.out.println(conf.getPath());
    }
    
    public ConfigCategory getTestConfig() throws UnsupportedConfigOperationException {
	ConfigCategory t = new ConfigCategory("test");
	t.addProperty(new Property("test1", "Hello", Property.Type.STRING));
	ConfigCategory t2 = new ConfigCategory("lists");
	String[] i = {"1", "2", "3", "4", "5"};
	String[] f = {"1.123", "2.345", "3.456", "4.567", "5.678"};
	String[] d = {"1.23", "2.34", "3.45", "4.56", "5.67"};
	String[] b = {"true", "false", "false", "true", "false"};
	String[] s = {"Hello", "World", "I", "Am", "A", "String"};
	
	Property in = new Property("int", i, Property.Type.INTEGER);
	Property fl = new Property("float", f, Property.Type.FLOAT);
	fl.setComment("This property has a comment!");
	Property db = new Property("double", d, Property.Type.DOUBLE);
	Property bl = new Property("boolean", b, Property.Type.BOOLEAN);
	Property st = new Property("string", s, Property.Type.STRING);
	
	t2.addProperties(in, fl, db, bl, st);
	t2.setComment("This category has a comment!");
	
	ConfigCategory t3 = new ConfigCategory("(sub)category with spaces");
	t3.addProperty(new Property("property with spaces", "spaces are cool!", Property.Type.STRING));
	
	t.addSubCategory(t2);
	t.addSubCategory(t3);
		
	t.addProperty(new Property("test2", "World!", Property.Type.STRING));
	
	return t;
    }
    
}

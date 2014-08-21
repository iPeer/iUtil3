package com.simple.ipeer.config;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStreamWriter;

/**
 *
 * @author iPeer
 */
public class Configuration {
    
    private File file;
    private ConfigCategory structure;
    private String filePath;
    
    public Configuration() {}
    
    public Configuration(String file) {
	this(new File(file));
    }
    
    public Configuration(File file) {
	this.file = file;
	this.filePath = file.getAbsolutePath().replace(File.separatorChar, '/').replace("/./", "/");
	try {
	    load();
	}
	catch (Throwable _e) {
	    //TODO
	}
    }
    
    public void load() {
	//TODO
    }
    
    @SuppressWarnings("CallToPrintStackTrace")
    public void save() {
	try {
	    if (!file.exists() && !file.createNewFile()) {
		System.err.println("Couldn't create config file at "+this.filePath);
		return;
	    }
	    if (file.canWrite()) {
		FileOutputStream fos = new FileOutputStream(this.file);
		BufferedWriter out = new BufferedWriter(new OutputStreamWriter(fos, "UTF-8"));
		writeSave(this.structure, 1, out);
		out.flush();
		out.close();
	    }
	    
	    
	}
	catch (IOException _e) { _e.printStackTrace(); }
    }
    
    public void writeSave(ConfigCategory cc, int i, BufferedWriter out) throws IOException {
	if (i < 1) { i = 1; }
	String str = "\t";
	String pre = new String(new char[i]).replace("\0", "\t");
	String preM1 = new String(new char[i - 1]).replace("\0", "\t");
	out.append(preM1+cc.getRegisteredName()+" {");
	out.append(System.lineSeparator());
	i++;
	if (cc.getProperties().length > 0)
	    for (Property a : cc.getProperties()) {
		if (a.hasComment()) {
		    out.append(pre+"# "+a.getComment());
		    out.append(System.lineSeparator());
		}
		out.append(pre+a.getType().getID()+":"+a.getRegisteredName()+"="+a.getValue());
		out.append(System.lineSeparator());
	    }
	if (cc.getSubCategories().length > 0) {
	    for (ConfigCategory c : cc.getSubCategories()) {
		if (c.hasComment()) {
		    out.append(pre+"# "+c.getComment());
		    out.append(System.lineSeparator());
		}
		writeSave(c, i, out);
	    }
	}
	out.append(preM1+"}");
	out.append(System.lineSeparator());
    }
    
    public Configuration setStructure(ConfigCategory cc) {
	System.out.println("[WARN] This method is designed to be used in testing situations only and should not be used by live code!");
	this.structure = cc;
	return this;
    }
    
    public String getPath() {
	return this.filePath;
    }
    
}

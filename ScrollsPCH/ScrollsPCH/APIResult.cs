using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class APIResult
{
	public String msg;
	public DataDeserialiser data;
}

public class DataDeserialiser
{
	public String id;
	public String name;
	public String price;
    public String price_max;
    public String live_price;
}
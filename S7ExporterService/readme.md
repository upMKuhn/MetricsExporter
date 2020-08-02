# S7ExporterService
This service reads regulary the variables from the S7 PLC and posts them via websocket to the Metrics Relay. 
The relay is then called from Prometheus on a regular interval.

# Deployment 
Place content of build directory into a Folder and Create a shortcut to the Auto Program Start folder. .... Make the app start on start up.

#Metrics Common
Common Classes for B&R / Zenon metrics, S7 PLC communication, Websocket Export

## How to update the s7.variables.json file
You need to export the variables from the Simatec TIA portal as a xlsx file.

Make sure that you Copy the Quellkommentar over. You can do that by copy in excel format and pasting it in.
Put the values into Display Name column!
Then Set the header names to. You can do that by pasting special in open office.
```
Name	Path	Connection	PLC_tag	DataType	Length	Coding	Access_Method	Address	Indirect_addressing	Index_tag	Start_value	ID_tag	Display_Name	Comment	Acquisition_mode	Acquisition_cycle	Limit_Upper_2_Type	Limit_Upper_2	Limit_Upper_1_Type	Limit_Upper_1	Limit_Lower_1_Type	Limit_Lower_1	Limit_Lower_2_Type	Limit_Lower_2	Linear_scaling	End_value_PLC	Start_value_PLC	End_value_HMI	Start_value_HMI	Gmp_relevant	Confirmation_Type	Mandatory_Commenting																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																																															
```

Then you can convert it into a json file using https://www.aconvert.com/document/xls-to-json/
To make it human readable use: https://jsonformatter.curiousconcept.com/


### s7.variables.patch
Here you can safely overwrite a property. Both files will be merged....
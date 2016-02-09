# Unity3D-ResourceAttribute
An attribute that let's you auto-load resources

* Works on fields and properties
* Loads single resource
* Loads multiple resources on arrays

##Code
Using this attribute is simple; you only need to add the Resource attribute on top of the fields or properties you want to set and call *this.LoadResources();*.
You can choose to specify the name of the resource you want to load, or, either leave it empty or make it end with a / *(forward slash)* to load all the resources in that folder.

![Imgur](http://i.imgur.com/26ZfaQl.png)

*Note: some resources might need the second parameter, forceType, set to true to be able to load properly due to casting of the object type.*

##Result
![Imgur](http://i.imgur.com/Hb2MFME.png)

Every time *this.LoadResources();* gets called it will attempt to load the resources into the resource objects. You are **not** limited to calling this function in a monobehaviour's Start method.

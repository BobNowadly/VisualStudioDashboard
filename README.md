# Visual Studio Dashboard

Currently, this tool is not really a dashboard but over the next couple of months I hope to build one. 

I created this project so that 

* I can report cycle time for each work item on Visual Studio Online 
* I can report the average cycle time grouped by effort

Eventually, I will have these reports display on a web page, in the meantime I created a console app that will export a csv file in the following format: 

<table> 
<tr><td>DateCommittedTime</td><td>DateClosed</td><td>Id</td><td>Title</td><td>Effort</td></tr>
<tr><td>7/9/2014 10:05</td><td>7/9/2014 9:26</td><td>86</td><td>Added Unit/Integration test project</td><td>1</td></tr>
</table>

Then I take this output and copy and paste it into an excel file that will do the calculations. 



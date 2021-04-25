SELECT 
   COUNT(*) CNT
FROM 
   [dbo].[EMP] 
WHERE 
    ID = /* Id */1
    /*%if Name != null && Name != "" */
    AND NAME LIKE /* @StartsWith(Name) */'A%'
    /*%end*/

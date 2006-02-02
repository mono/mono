DROP FUNCTION GH_DUMMY(CHAR);


CREATE FUNCTION GH_DUMMY(CHAR) RETURNS refcursor AS $$
DECLARE 
   rct1 refcursor;
 
BEGIN
  OPEN rct1 FOR
	SELECT * FROM Customers where CustomerID = $1;
  RETURN rct1;
END;
$$ LANGUAGE 'plpgsql';
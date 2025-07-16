DECLARE @data NVARCHAR(MAX);
DECLARE @info NVARCHAR(MAX);
DECLARE @statusCode NVARCHAR(MAX);
DECLARE @messages NVARCHAR(MAX);

BEGIN TRY
    WITH MenuTree AS (
        SELECT
            mf.ID,
            mf.ID_MENU,
            mf.NAME_MENU,
            mf.SHOWING_LABEL,
            mf.TYPE_MENU,
            mf.CHILD_MENU,
            mf.IS_ACTIVATED
        FROM MASTER_FUNCTION mf
        WHERE mf.CHILD_MENU = 0
    )
    
    SELECT @data = (
        SELECT 
            mt.ID,
            mt.ID_MENU,
            mt.NAME_MENU,
            mt.SHOWING_LABEL,
            mt.TYPE_MENU,
            mt.IS_ACTIVATED,
            (
                SELECT 
                    c.ID,
                    c.ID_MENU,
                    c.NAME_MENU,
                    c.SHOWING_LABEL,
                    c.TYPE_MENU,
                    c.IS_ACTIVATED
                FROM MASTER_FUNCTION c
                WHERE c.CHILD_MENU = mt.ID 
                FOR JSON PATH
            ) AS sub_menus
        FROM MenuTree mt 
        FOR JSON PATH
    );

    SET @info = 'SUCCESS';
    SET @statusCode = 200;
    SET @messages = 'JSON SUCCESS';
END TRY 
BEGIN CATCH
    SET @info = 'ERROR';
    SET @statusCode = 500;
    SET @messages = ERROR_MESSAGE();
    SET @data = NULL;
END CATCH;

SELECT 
    @info AS msgInfo,
    @statusCode AS statusCode,
    @messages AS msg,
    @data AS data;

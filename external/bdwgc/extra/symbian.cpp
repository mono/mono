
#include <e32cmn.h>
#include <e32std.h>
#include <f32file.h>
#include <aknutils.h>
#include <stdlib.h>
#include <string.h>

extern "C" {

int GC_get_main_symbian_stack_base()
{
    TThreadStackInfo aInfo;
    TInt err = RThread().StackInfo(aInfo);
    if ( !err )
        {
        return aInfo.iBase;
        }
    else
        {
        return 0;
        }
}

char* GC_get_private_path_and_zero_file()
{
    // always on c: drive
    RFs fs;
    fs.Connect();
    fs.CreatePrivatePath( EDriveC );
    TFileName path;
    fs.PrivatePath( path );
    fs.Close();
    _LIT( KCDrive, "c:" );
    path.Insert( 0, KCDrive );


    //convert to char*, assume ascii
    TBuf8<KMaxFileName> path8;
    path8.Copy( path );
    _LIT8( KZero8, "zero" );
    path8.Append( KZero8 );

    size_t size = path8.Length() + 1;
    char* copyChar = (char*) malloc( size );
    if (copyChar)
        memcpy( copyChar, path8.PtrZ(), size );

    return copyChar; // ownership passed
}

} /* extern "C" */

ASSEMBLY = Mtp
TARGET = library
LINK = $(REF_MTP)

BUILD_DEFINES = ""

if LIBMTP_SIZEOF_TIME_T_64
BUILD_DEFINES += "-define:LIBMTP_SIZEOF_TIME_T_64"
endif

SOURCES =  \
	Mtp/AbstractTrackList.cs \
	Mtp/Album.cs \
	Mtp/Error.cs \
	Mtp/ErrorCode.cs \
	Mtp/File.cs \
	Mtp/FileSampleData.cs \
	Mtp/FileType.cs \
	Mtp/Folder.cs \
	Mtp/MtpDevice.cs \
	Mtp/Playlist.cs \
	Mtp/Track.cs

if ENABLE_MTP
include $(top_srcdir)/build/build.mk
module_SCRIPTS += Mtp.dll.config
else
include $(top_srcdir)/build/build.dist.mk
endif

EXTRA_DIST += Mtp.dll.config.in
DISTCLEANFILES += Mtp.dll.config

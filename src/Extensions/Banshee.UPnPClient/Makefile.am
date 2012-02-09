ASSEMBLY = Banshee.UPnPClient
TARGET = library
LINK = $(REF_EXTENSION_UPNP)
INSTALL_DIR = $(EXTENSIONS_INSTALL_DIR)

SOURCES =  \
	Banshee.UPnPClient/UPnPContainerSource.cs \
	Banshee.UPnPClient/UPnPMusicSource.cs \
	Banshee.UPnPClient/UPnPServerSource.cs \
	Banshee.UPnPClient/UPnPService.cs \
	Banshee.UPnPClient/UPnPTrackInfo.cs \
	Banshee.UPnPClient/UPnPVideoSource.cs

RESOURCES = Banshee.UPnPClient.addin.xml

if UPNP_ENABLED
include $(top_srcdir)/build/build.mk
else
include $(top_srcdir)/build/build.dist.mk
endif


LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := lua

LOCAL_MODULE_FILENAME := liblua

LOCAL_SRC_FILES := $(wildcard $(LOCAL_PATH)/../../src/*.c)
LOCAL_SRC_FILES += $(wildcard $(LOCAL_PATH)/../../src/*.cpp)
LOCAL_SRC_FILES := $(LOCAL_SRC_FILES:$(LOCAL_PATH)/%=%)

$(info LOCAL_PATH= $(LOCAL_PATH))
$(info LOCAL_SRC_FILES= $(LOCAL_SRC_FILES))

LOCAL_SRC_FILES := $(subst  ../../src/lua.c,,$(LOCAL_SRC_FILES)) 
LOCAL_SRC_FILES := $(subst  ../../src/luac.c,,$(LOCAL_SRC_FILES)) 
$(info LOCAL_SRC_FILES= $(LOCAL_SRC_FILES))

# LOCAL_EXPORT_C_INCLUDES := $(LOCAL_PATH)/../include/lua

LOCAL_CFLAGS += -DLUA_USE_LINUX

# LOCAL_LDLIBS += -lreadline

include $(BUILD_SHARED_LIBRARY)

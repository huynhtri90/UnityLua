LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := lua

LOCAL_MODULE_FILENAME := liblua

LOCAL_SRC_FILES := $(wildcard $(LOCAL_PATH)/../src/*.c)
LOCAL_SRC_FILES += $(wildcard $(LOCAL_PATH)/../src/*.cpp)
LOCAL_SRC_FILES := $(LOCAL_SRC_FILES:$(LOCAL_PATH)/%=%)

# $(info SRC= $(LOCAL_SRC_FILES))

# LOCAL_EXPORT_C_INCLUDES := $(LOCAL_PATH)/../include/lua

LOCAL_CFLAGS += -DLUA_USE_LINUX

include $(BUILD_SHARED_LIBRARY)

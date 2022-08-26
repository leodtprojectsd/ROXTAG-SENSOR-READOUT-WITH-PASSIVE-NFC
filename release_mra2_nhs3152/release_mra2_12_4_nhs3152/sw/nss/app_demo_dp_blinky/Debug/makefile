################################################################################
# Automatically-generated file. Do not edit!
################################################################################

-include ../makefile.init

RM := rm -rf

# All of the sources participating in the build are defined here
-include sources.mk
-include src/subdir.mk
-include subdir.mk
-include objects.mk

ifneq ($(MAKECMDGOALS),clean)
ifneq ($(strip $(C_DEPS)),)
-include $(C_DEPS)
endif
endif

-include ../makefile.defs

# Add inputs and outputs from these tool invocations to the build variables 

# All Target
all: app_demo_dp_blinky.axf

dependents:
	-cd C:/Users/leodavide.torchia2/Downloads/release_mra2_nhs3152/release_mra2_12_4_nhs3152/sw/nss/lib_board_dp/Debug && $(MAKE) all
	-cd C:/Users/leodavide.torchia2/Downloads/release_mra2_nhs3152/release_mra2_12_4_nhs3152/sw/nss/lib_chip_nss/Debug && $(MAKE) all

# Tool invocations
app_demo_dp_blinky.axf: $(OBJS) $(USER_OBJS) C:/Users/leodavide.torchia2/Downloads/release_mra2_nhs3152/release_mra2_12_4_nhs3152/sw/nss/lib_board_dp/Debug/liblib_board_dp.a C:/Users/leodavide.torchia2/Downloads/release_mra2_nhs3152/release_mra2_12_4_nhs3152/sw/nss/lib_chip_nss/Debug/liblib_chip_nss.a
	@echo 'Building target: $@'
	@echo 'Invoking: MCU Linker'
	arm-none-eabi-gcc -nostdlib -L"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\lib_board_dp\Debug" -L"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\lib_chip_nss\Debug" -Xlinker -Map="app_demo_dp_blinky.map" -Xlinker --gc-sections -mcpu=cortex-m0 -mthumb -T "app_demo_dp_blinky_Debug.ld" -o "app_demo_dp_blinky.axf" $(OBJS) $(USER_OBJS) $(LIBS)
	@echo 'Finished building target: $@'
	@echo ' '
	$(MAKE) --no-print-directory post-build

# Other Targets
clean:
	-$(RM) $(EXECUTABLES)$(OBJS)$(C_DEPS) app_demo_dp_blinky.axf
	-@echo ' '

post-build:
	-@echo 'Performing post-build steps'
	-arm-none-eabi-size "app_demo_dp_blinky.axf" ; arm-none-eabi-objcopy -v -O binary "app_demo_dp_blinky.axf" "app_demo_dp_blinky.bin" ; arm-none-eabi-objcopy -v -O ihex "app_demo_dp_blinky.axf" "app_demo_dp_blinky.hex" ; # checksum -p NHS3100 -d "app_demo_dp_blinky.bin"
	-@echo ' '

.PHONY: all clean dependents
.SECONDARY: post-build
C:/Users/leodavide.torchia2/Downloads/release_mra2_nhs3152/release_mra2_12_4_nhs3152/sw/nss/lib_board_dp/Debug/liblib_board_dp.a:
C:/Users/leodavide.torchia2/Downloads/release_mra2_nhs3152/release_mra2_12_4_nhs3152/sw/nss/lib_chip_nss/Debug/liblib_chip_nss.a:

-include ../makefile.targets

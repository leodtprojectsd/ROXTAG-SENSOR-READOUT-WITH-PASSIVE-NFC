################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
C:/Users/leodavide.torchia2/Downloads/release_mra2_nhs3152/release_mra2_12_4_nhs3152/sw/nss/mods/diag/diag.c 

OBJS += \
./mods/diag/diag.o 

C_DEPS += \
./mods/diag/diag.d 


# Each subdirectory must supply rules for building sources it contributes
mods/diag/diag.o: C:/Users/leodavide.torchia2/Downloads/release_mra2_nhs3152/release_mra2_12_4_nhs3152/sw/nss/mods/diag/diag.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -std=c99 -D__REDLIB__ -DDEBUG -D__CODE_RED -DCORE_M0PLUS -D"DIAG_HEADER=0x$(shell date +%M)" -I"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\lib_chip_nss\inc" -I"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\lib_chip_nss\mods" -I"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\mods" -include"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\lib_chip_nss\mods\chip_sel.h" -Og -g3 -pedantic -Wall -Wextra -Wconversion -c -fmessage-length=0 -fno-builtin -ffunction-sections -fdata-sections -mcpu=cortex-m0plus -mthumb -D__REDLIB__ -specs=redlib.specs -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.o)" -MT"$(@:%.o=%.d)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '



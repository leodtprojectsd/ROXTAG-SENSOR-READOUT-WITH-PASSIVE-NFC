################################################################################
# Automatically-generated file. Do not edit!
################################################################################

# Add inputs and outputs from these tool invocations to the build variables 
C_SRCS += \
../src/board.c 

OBJS += \
./src/board.o 

C_DEPS += \
./src/board.d 


# Each subdirectory must supply rules for building sources it contributes
src/%.o: ../src/%.c
	@echo 'Building file: $<'
	@echo 'Invoking: MCU C Compiler'
	arm-none-eabi-gcc -std=c99 -D__REDLIB__ -DDEBUG -D__CODE_RED -DCORE_M0PLUS -I"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\lib_chip_nss" -I"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\lib_board_dp\inc" -I"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\lib_board_dp\mods" -I"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\lib_chip_nss\inc" -I"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\lib_chip_nss\mods" -I"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\mods" -include"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\lib_chip_nss\mods\chip_sel.h" -include"C:\Users\leodavide.torchia2\Downloads\release_mra2_nhs3152\release_mra2_12_4_nhs3152\sw\nss\lib_board_dp\mods\board_sel.h" -Og -g3 -pedantic -Wall -Wextra -Wconversion -c -fmessage-length=0 -fno-builtin -ffunction-sections -fdata-sections -mcpu=cortex-m0plus -mthumb -D__REDLIB__ -specs=redlib.specs -MMD -MP -MF"$(@:%.o=%.d)" -MT"$(@:%.o=%.o)" -MT"$(@:%.o=%.d)" -o "$@" "$<"
	@echo 'Finished building: $<'
	@echo ' '



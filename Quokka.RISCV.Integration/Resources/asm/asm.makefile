elf: sections.lds test.S
	riscv32-unknown-elf-gcc -march=rv32i -nostartfiles -Wl,-Bstatic,-T,sections.lds,--strip-debug,-Map=firmware.map,--cref  -ffreestanding -nostdlib -o firmware.elf test.S

bin: elf
	riscv32-unknown-elf-objcopy -O binary firmware.elf /dev/stdout > firmware.bin

clean:
	rm -f firmware.elf firmware.bin firmware.map

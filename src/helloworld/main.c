#include <efi.h>
#include <efilib.h>

EFI_STATUS efi_main(EFI_HANDLE imageHandle, EFI_SYSTEM_TABLE* systemTable)
{
	// initialize GNU EFI library

	InitializeLib(imageHandle, systemTable);

	// do something

	Print(L"Hello world!\n");
	Print(L"Firmware vendor: %s\n", systemTable->FirmwareVendor);

	// show prompt

	Print(L"\nPress any key to reboot...\n");

	// wait any key

	ST->ConIn->Reset(ST->ConIn, FALSE);

	UINTN keyEvent;
	BS->WaitForEvent(1, &ST->ConIn->WaitForKey, &keyEvent);

	EFI_INPUT_KEY key;
	ST->ConIn->ReadKeyStroke(ST->ConIn, &key);

	// reboot device
	
	ST->RuntimeServices->ResetSystem(EfiResetCold, EFI_SUCCESS, 0, NULL);

	return EFI_SUCCESS;
}

import {
    Directive,
    ElementRef,
    forwardRef,
    HostListener,
    Input
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Directive({
    selector: '[appCurrencyFormat]',
    // standalone: true,
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => CurrencyFormatDirective),
            multi: true
        }
    ]
})
export class CurrencyFormatDirective implements ControlValueAccessor {

    @Input() precision = 2;

    private onChange!: (value: number | null) => void;
    private onTouched!: () => void;

    constructor(private el: ElementRef<HTMLInputElement>) { }

    // ===============================
    // FORM -> VIEW
    // ===============================
    writeValue(value: number | null): void {
        if (value == null) {
            this.el.nativeElement.value = '';
            return;
        }

        this.el.nativeElement.value = this.format(value);
    }

    registerOnChange(fn: any): void {
        this.onChange = fn;
    }

    registerOnTouched(fn: any): void {
        this.onTouched = fn;
    }

    // ===============================
    // INPUT
    // ===============================
    @HostListener('input')
    onInput(): void {
        const input = this.el.nativeElement;
        const rawValue = input.value;

        // Nếu đang nhập phần thập phân thì không format lại
        if (rawValue.includes(',')) {
            this.onChange(this.parse(rawValue));
            return;
        }

        const numericValue = this.parse(rawValue);
        this.onChange(numericValue);

        if (numericValue !== null) {
            input.value = this.format(numericValue);
        }
    }

    @HostListener('blur')
    onBlur(): void {
        const numericValue = this.parse(this.el.nativeElement.value);

        if (numericValue !== null) {
            this.el.nativeElement.value = this.format(numericValue);
        }

        this.onTouched();
    }

    // ===============================
    // PARSE
    // ===============================
    private parse(value: string): number | null {
        if (!value) return null;

        // bỏ dấu chấm hàng nghìn
        let cleaned = value.replace(/\./g, '');

        // đổi , thành .
        cleaned = cleaned.replace(',', '.');

        const number = Number(cleaned);

        return isNaN(number) ? null : number;
    }

    // ===============================
    // FORMAT
    // ===============================
    private format(value: number): string {
        return new Intl.NumberFormat('vi-VN', {
            minimumFractionDigits: 0,
            maximumFractionDigits: this.precision
        }).format(value);
    }
}

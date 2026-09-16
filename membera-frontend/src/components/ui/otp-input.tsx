"use client";
import { cn } from "@/lib/utils";
import { OTPInput, type SlotProps } from "input-otp";

// Same glass shell used by GlassInputWrapper in sign-in.tsx/sign-up.tsx
// (border-border + bg-foreground/5 + backdrop-blur-sm), with the same
// blue-400/blue-500 focus tint swapped in per slot instead of per field.
function Slot(props: SlotProps) {
  return (
    <div
      className={cn(
        "flex size-12 items-center justify-center rounded-2xl border border-border bg-foreground/5 text-lg font-medium text-neutral-900 shadow-sm shadow-blue-500/5 backdrop-blur-sm transition-colors",
        { "z-10 border-blue-400/70 bg-blue-500/10 ring-[3px] ring-blue-400/20": props.isActive },
      )}
    >
      {props.char !== null && <div>{props.char}</div>}
    </div>
  );
}

export function OtpInput({
  value,
  onChange,
  disabled,
}: {
  value: string;
  onChange: (v: string) => void;
  disabled?: boolean;
}) {
  return (
    <OTPInput
      value={value}
      onChange={onChange}
      disabled={disabled}
      containerClassName="flex items-center gap-3 has-[:disabled]:opacity-50"
      maxLength={6}
      inputMode="numeric"
      render={({ slots }) => (
        <div className="flex gap-2">
          {slots.map((slot, idx) => (
            <Slot key={idx} {...slot} />
          ))}
        </div>
      )}
    />
  );
}

import {
  Directive,
  Input,
  TemplateRef,
  ViewContainerRef,
  OnInit,
} from '@angular/core';
import { PermissionService } from '../utils/services/permission.service';

@Directive({
  selector: '[appHasPermission]',
})
export class HasPermissionDirective implements OnInit {
  @Input('appHasPermission') permission!: string | string[];
  @Input() mode: 'hide' | 'disable' = 'hide';

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private permissionService: PermissionService
  ) { }

  ngOnInit(): void {
    const hasPermission = Array.isArray(this.permission)
      ? this.permissionService.hasAnyPermission(this.permission)
      : this.permissionService.hasPermission(this.permission);

    if (hasPermission) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      if (this.mode === 'hide') {
        this.viewContainer.clear();
      } else {
        // render nhưng disable
        const view = this.viewContainer.createEmbeddedView(this.templateRef);
        setTimeout(() => {
          const element = view.rootNodes[0];
          if (element) {
            element.disabled = true;
            element.style.pointerEvents = 'none';
            element.style.opacity = '0.5';
          }
        });
      }
    }
  }
}
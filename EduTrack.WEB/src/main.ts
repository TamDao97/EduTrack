import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { registerLocaleData } from '@angular/common';
import vi from '@angular/common/locales/vi';
import { ModuleRegistry, AllCommunityModule } from 'ag-grid-community';

registerLocaleData(vi); // Đăng ký locale cho Angular
// Register all Community features
ModuleRegistry.registerModules([AllCommunityModule]);
bootstrapApplication(AppComponent, appConfig).catch((err) =>
  console.error(err)
);

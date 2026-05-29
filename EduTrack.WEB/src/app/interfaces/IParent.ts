import { IBase, IGridFilterBase } from '../shared/interfaces/IBase-ext';

export interface IParent extends IBase {
  idTutor?: string;
  fullName: string;
  phone: string;
  email?: string;
  notes?: string;
}

export interface IParentGridFilter extends IGridFilterBase {
}

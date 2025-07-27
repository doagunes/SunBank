import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Bills } from './bills';

describe('Bills', () => {
  let component: Bills;
  let fixture: ComponentFixture<Bills>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Bills]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Bills);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

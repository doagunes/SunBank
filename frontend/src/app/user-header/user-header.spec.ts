import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserHeader } from './user-header';

describe('UserHeader', () => {
  let component: UserHeader;
  let fixture: ComponentFixture<UserHeader>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserHeader]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserHeader);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

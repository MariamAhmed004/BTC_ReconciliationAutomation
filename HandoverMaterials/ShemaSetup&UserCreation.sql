CREATE USER billing_mock IDENTIFIED BY billing123;

GRANT CONNECT, RESOURCE TO billing_mock;


GRANT CREATE SESSION TO billing_mock;
GRANT CREATE TABLE TO billing_mock;
GRANT CREATE VIEW TO billing_mock;
GRANT CREATE SEQUENCE TO billing_mock;
GRANT CREATE PROCEDURE TO billing_mock;


ALTER USER billing_mock DEFAULT TABLESPACE users;
ALTER USER billing_mock QUOTA UNLIMITED ON users;


ALTER USER billing_mock ACCOUNT UNLOCK;




BEGIN
  DBMS_NETWORK_ACL_ADMIN.create_acl (
    acl         => 'mail_acl.xml',
    description => 'Allow access to SMTP',
    principal   => 'SYSTEM',   -- schema user
    is_grant    => TRUE,
    privilege   => 'connect'
  );
END;
/

BEGIN
  DBMS_NETWORK_ACL_ADMIN.assign_acl (
    acl  => 'mail_acl.xml',
    host => 'localhost',
    lower_port => 25,
    upper_port => 25
  );
END;
/


COMMIT;

BEGIN
  DBMS_NETWORK_ACL_ADMIN.add_privilege (
    acl       => 'mail_acl.xml',   -- same ACL  created earlier
    principal => 'BILLING_MOCK',   --  schema user
    is_grant  => TRUE,
    privilege => 'connect'
  );
END;
/
COMMIT;
